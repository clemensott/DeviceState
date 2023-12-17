using System;
using DeviceStateWeb.Models;

namespace DeviceStateWeb.Extensions.Services
{
    static class DeviceOnStateExtensions
    {
        public static TimeSpan? GetRemaining(this Device device)
        {
            DateTime now = DateTime.UtcNow;
            return device.IsForeverOn && device.OnUntil < now ? null : device.OnUntil - now;
        }

        public static bool IsOn(this Device device)
        {
            return device.IsForeverOn || device.OnUntil >= DateTime.UtcNow;
        }
    }
}
