using System;

namespace DeviceStateWeb.Models
{
    public record DeviceDesiredOnState(bool IsForeverOn, DateTime OnUntil);
}
