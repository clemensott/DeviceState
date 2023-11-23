using System;

namespace WaterpumpWeb.Models
{
    public record DeviceOnState(bool IsForeverOn, DateTime OnUntil);
}
