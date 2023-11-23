using System;
using System.Threading.Tasks;
using WaterpumpWeb.Models;

namespace WaterpumpWeb.Services.Devices
{
    public interface IDevicesService
    {
        Task TurnOn(string id, double? millis, double? time);

        Task TurnOff(string id);

        Task<bool> SetMeasurements(string id, int? errors, int? state, int? value, TimeSpan? maxWaitTime);

        Task<TimeSpan?> GetRemainingOnTime(string id);

        Task<DeviceState> GetState(string id, TimeSpan? maxWaitTime);
    }
}
