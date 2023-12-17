using System;

namespace DeviceStateWeb.Models
{
    public record Device(string Name, bool IsForeverOn, DateTime OnUntil, TimeSpan LastValuesSpan, string ValueName, DateTime? LasteActorUpdate);
}
