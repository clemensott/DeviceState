using System;
using System.Linq;
using System.Threading.Tasks;
using WaterpumpWeb.Extensions.Services;
using WaterpumpWeb.Models;
using WaterpumpWeb.Models.Exceptions;

namespace WaterpumpWeb.Services.Devices
{
    public class DevicesService : IDevicesService
    {
        private readonly string defaultDeviceId;
        private readonly IDevicesEvents devicesEvents;
        private readonly IDeviceRepo deviceRepo;

        public DevicesService(IAppConfiguration appConfiguration, IDevicesEvents devicesEvents, IDeviceRepo deviceRepo)
        {
            defaultDeviceId = appConfiguration.DefaultDeviceId;
            this.devicesEvents = devicesEvents;
            this.deviceRepo = deviceRepo;
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
            DateTime? lastUpdate = measurements.Length > 0 ? measurements[0].Created : null;

            double[] measurementValues = measurements.Select(m => m.Value).OfType<double>().ToArray();
            TransformedValue transformedValue;
            if (measurementValues.Length > 0)
            {
                double averageValue = measurementValues.Average();
                transformedValue = TemperatureConverter.Convert(device.ValueName, averageValue);
            }
            else transformedValue = TransformedValue.Empty(device.ValueName);

            return new DeviceState(id, device.Name, isOn, device.IsForeverOn, 
                device.OnUntil, lastUpdate, transformedValue);
        }

        public async Task<bool> SetMeasurements(string id, int? errors, int? state, int? value, TimeSpan? maxWaitTime)
        {
            id ??= defaultDeviceId;

            DeviceMeasurement measurement = new DeviceMeasurement(id, errors, state, value);

            await deviceRepo.SetMeasurement(measurement);
            devicesEvents.TriggerStateChange(id);


            if (maxWaitTime.HasValue)
            {
                await devicesEvents.WaitForStateChange(id, maxWaitTime.Value);
            }

            Device device = await deviceRepo.GetDevice(id);
            bool isOn = device.IsOn();

            return isOn;
        }

        public async Task TurnOff(string id)
        {
            id ??= defaultDeviceId;
            await deviceRepo.SetDeviceOnState(id, new DeviceOnState(false, DateTime.MinValue));
            devicesEvents.TriggerStateChange(id);
        }

        public async Task TurnOn(string id, double? millis, double? minutes)
        {
            id ??= defaultDeviceId;

            DeviceOnState onState;
            if (millis.HasValue)
            {
                TimeSpan onTime = millis < 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(millis.Value);
                onState = new DeviceOnState(false, DateTime.UtcNow + onTime);
            }
            else if (minutes.HasValue)
            {
                TimeSpan onTime = millis < 0 ? TimeSpan.Zero : TimeSpan.FromMinutes(minutes.Value);
                onState = new DeviceOnState(false, DateTime.UtcNow + onTime);
            }
            else onState = new DeviceOnState(true, DateTime.MaxValue);

            bool hasDevice = await deviceRepo.SetDeviceOnState(id, onState);
            if (!hasDevice) throw new DeviceNotFoundException(id);

            devicesEvents.TriggerStateChange(id);
        }
    }
}
