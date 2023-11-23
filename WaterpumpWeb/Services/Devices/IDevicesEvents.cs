using System;
using System.Threading.Tasks;

namespace WaterpumpWeb.Services.Devices
{
    public interface IDevicesEvents
    {
        Task<bool> WaitForStateChange(string deviceId, TimeSpan maxWaitTime);

        void TriggerStateChange(string deviceId);
    }
}
