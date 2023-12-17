using System;

namespace DeviceStateWeb.Models
{
    public record DeviceOnState(bool IsForeverOn, DateTime OnUntil, bool? IsOn)
        : DeviceDesiredOnState(IsForeverOn, OnUntil);
}
