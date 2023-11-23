using System;

namespace WaterpumpWeb.Models.Exceptions
{
    public class DeviceNotFoundException : Exception
    {
        public string DeviceId { get; }

        public DeviceNotFoundException(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
