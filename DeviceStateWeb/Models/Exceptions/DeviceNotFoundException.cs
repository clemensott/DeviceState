using System;

namespace DeviceStateWeb.Models.Exceptions
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
