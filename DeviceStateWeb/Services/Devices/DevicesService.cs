using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceStateWeb.Extensions.Services;
using DeviceStateWeb.Models;
using DeviceStateWeb.Models.Exceptions;
using DeviceStateWeb.Services.ValueTransformers;

namespace DeviceStateWeb.Services.Devices
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

        public Task<Device> GetDevice(string id)
        {
            id ??= defaultDeviceId;

            return deviceRepo.GetDevice(id);
        }

        public async Task<TimeSpan?> GetRemainingOnTime(string id)
        {
            id ??= defaultDeviceId;

            Device device = await deviceRepo.GetDevice(id);
            return device.GetRemaining();
        }

        private async Task<DeviceActorOnline> GetIsActorOnline(string id, TimeSpan? maxWaitTime)
        {
            DeviceActorOnline actorOnline = devicesEvents.GetLastActorUpdate(id);
            if (maxWaitTime.HasValue)
            {
                Task waitTask = devicesEvents.WaitForStateChange(id, maxWaitTime.Value);
                while (!waitTask.IsCompleted)
                {
                    DateTime delayUntil = (actorOnline.LastUpdate ?? DateTime.UtcNow) + actorOnlineTolerance;
                    TimeSpan delay = delayUntil - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero) await Task.WhenAny(waitTask, Task.Delay(delay));
                    else await waitTask;

                    actorOnline = devicesEvents.GetLastActorUpdate(id);
                    if (!actorOnline.IsOnline) break;
                }
            }

            Device device = await deviceRepo.GetDevice(id) ?? throw new DeviceNotFoundException(id);
            DateTime? actorOnlineLastUpdate = actorOnline.LastUpdate ?? device.LasteActorUpdate;
            bool actorIsOnline = actorOnline.IsOnline || (actorOnlineLastUpdate.HasValue
                && DateTime.UtcNow - actorOnlineLastUpdate < actorOnlineTolerance);
            actorOnline = new DeviceActorOnline(actorIsOnline, actorOnlineLastUpdate);
            return actorOnline;
        }

        public async Task<DeviceState> GetState(string id, TimeSpan? maxWaitTime)
        {
            id ??= defaultDeviceId;

            DeviceActorOnline actorOnline = await GetIsActorOnline(id, maxWaitTime);
            Device device = await deviceRepo.GetDevice(id) ?? throw new DeviceNotFoundException(id);
            DeviceMeasurement[] measurements = await deviceRepo.GetMeasurements(id, device.LastValuesSpan);

            int? lastState = measurements.FirstOrDefault(m => m.State.HasValue).State;
            bool? isOn = lastState.HasValue ? lastState == 1 : null;

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

        public async Task<bool> SetMeasurements(string id, int? errors, int? state, int? value,
            TimeSpan? maxWaitTime, CancellationToken cancellationToken)
        {
            id ??= defaultDeviceId;

            DeviceMeasurement measurement = new DeviceMeasurement(id, errors, state, value);

            await deviceRepo.SetMeasurement(measurement);
            await deviceRepo.SetLastActorUpdate(id, DateTime.UtcNow);

            Device device = await deviceRepo.GetDevice(id);
            bool isOn = device.IsOn();
            if (state.HasValue && (state == 0) ^ isOn && maxWaitTime.HasValue)
            {
                if (await devicesEvents.ActorWaitForStateChange(id, maxWaitTime.Value, cancellationToken))
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

        public async Task TurnOn(string id, double? millis, double? minutes, bool defaultTime)
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
            else if (defaultTime)
            {
                Device device = await deviceRepo.GetDevice(id);
                onState = new DeviceDesiredOnState(false, DateTime.UtcNow + device.DefaultOnTime);
            }
            else onState = new DeviceDesiredOnState(true, DateTime.MaxValue);

            bool hasDevice = await deviceRepo.SetDeviceOnState(id, onState);
            if (!hasDevice) throw new DeviceNotFoundException(id);

            devicesEvents.TriggerStateChange(id);
        }
    }
}
