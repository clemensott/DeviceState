using System;
using System.Linq;
using System.Threading.Tasks;
using WaterpumpWeb.Extensions.Services;
using WaterpumpWeb.Models;
using WaterpumpWeb.Models.Exceptions;
using WaterpumpWeb.Services.ValueTransformers;

namespace WaterpumpWeb.Services.Devices
{
    public class DevicesService : IDevicesService
    {
        private readonly string defaultDeviceId;
        private readonly TimeSpan actorOnlineTolerance;
        private readonly IDevicesEvents devicesEvents;
        private readonly IDeviceRepo deviceRepo;
        private readonly IValueTransformer valueTransformer;

        public DevicesService(IAppConfiguration appConfiguration, IDevicesEvents devicesEvents,
            IDeviceRepo deviceRepo, IValueTransformer valueTransformer)
        {
            defaultDeviceId = appConfiguration.DefaultDeviceId;
            actorOnlineTolerance = appConfiguration.ActorOnlineTolerance;
            this.devicesEvents = devicesEvents;
            this.deviceRepo = deviceRepo;
            this.valueTransformer = valueTransformer;
        }

        public async Task<TimeSpan?> GetRemainingOnTime(string id)
        {
            id ??= defaultDeviceId;

            Device device = await deviceRepo.GetDevice(id);
            return device.GetRemaining();
        }

        public async Task<DeviceState> GetState(string id, TimeSpan? maxWaitTime)
        {
            id ??= defaultDeviceId;

            if (maxWaitTime.HasValue)
            {
                await devicesEvents.WaitForStateChange(id, maxWaitTime.Value);
            }

            Device device = await deviceRepo.GetDevice(id);
            if (device == null) throw new DeviceNotFoundException(id);

            DeviceMeasurement[] measurements = await deviceRepo.GetMeasurements(id, device.LastValuesSpan);

            int? lastState = measurements.FirstOrDefault(m => m.State.HasValue).State;
            bool? isOn = lastState.HasValue ? lastState == 1 : null;

            DeviceActorOnline actorOnline = devicesEvents.GetLastActorUpdate(id);
            DateTime? actorOnlineLastUpdate = actorOnline.LastUpdate ?? device.LasteActorUpdate;
            bool actorIsOnline = actorOnline.IsOnline || (actorOnlineLastUpdate.HasValue
                && DateTime.UtcNow - actorOnlineLastUpdate < actorOnlineTolerance);
            actorOnline = new DeviceActorOnline(actorIsOnline, actorOnlineLastUpdate);

            double[] measurementValues = measurements.Select(m => m.Value).OfType<double>().ToArray();
            TransformedValue transformedValue;
            if (measurementValues.Length > 0)
            {
                double averageValue = measurementValues.Average();
                transformedValue = valueTransformer.Transform(device.ValueName, averageValue);
            }
            else transformedValue = TransformedValue.Empty(device.ValueName);

            DeviceOnState onState = new DeviceOnState(device.IsForeverOn, device.OnUntil, isOn);
            return new DeviceState(id, device.Name, onState, actorOnline, transformedValue);
        }

        public async Task<bool> SetMeasurements(string id, int? errors, int? state, int? value, TimeSpan? maxWaitTime)
        {
            id ??= defaultDeviceId;

            DeviceMeasurement measurement = new DeviceMeasurement(id, errors, state, value);

            await deviceRepo.SetMeasurement(measurement);
            await deviceRepo.SetLastActorUpdate(id, DateTime.UtcNow);

            Device device = await deviceRepo.GetDevice(id);
            bool isOn = device.IsOn();
            if (state.HasValue && (state == 0) ^ isOn && maxWaitTime.HasValue)
            {
                if (await devicesEvents.ActorWaitForStateChange(id, maxWaitTime.Value))
                {
                    device = await deviceRepo.GetDevice(id);
                    isOn = device.IsOn();
                }

                await deviceRepo.SetLastActorUpdate(id, DateTime.UtcNow);
            }

            devicesEvents.TriggerStateChange(id);
            return isOn;
        }

        public async Task TurnOff(string id)
        {
            id ??= defaultDeviceId;
            await deviceRepo.SetDeviceOnState(id, new DeviceDesiredOnState(false, DateTime.MinValue));
            devicesEvents.TriggerStateChange(id);
        }

        public async Task TurnOn(string id, double? millis, double? minutes)
        {
            id ??= defaultDeviceId;

            DeviceDesiredOnState onState;
            if (millis.HasValue)
            {
                TimeSpan onTime = millis < 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(millis.Value);
                onState = new DeviceDesiredOnState(false, DateTime.UtcNow + onTime);
            }
            else if (minutes.HasValue)
            {
                TimeSpan onTime = millis < 0 ? TimeSpan.Zero : TimeSpan.FromMinutes(minutes.Value);
                onState = new DeviceDesiredOnState(false, DateTime.UtcNow + onTime);
            }
            else onState = new DeviceDesiredOnState(true, DateTime.MaxValue);

            bool hasDevice = await deviceRepo.SetDeviceOnState(id, onState);
            if (!hasDevice) throw new DeviceNotFoundException(id);

            devicesEvents.TriggerStateChange(id);
        }
    }
}
