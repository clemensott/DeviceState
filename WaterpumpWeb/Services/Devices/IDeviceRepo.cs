using System;
using System.Threading.Tasks;
using WaterpumpWeb.Models;

namespace WaterpumpWeb.Services.Devices
{
    public interface IDeviceRepo
    {
        Task<Device> GetDevice(string id);

        Task<bool> SetDeviceOnState(string id, DeviceOnState onState);

        Task<DeviceMeasurement[]> GetMeasurements(string id, TimeSpan last);

        Task SetMeasurement(DeviceMeasurement measurement);
    }
}
