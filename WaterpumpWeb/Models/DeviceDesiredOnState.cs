using System;

namespace WaterpumpWeb.Models
{
    public record DeviceDesiredOnState(bool IsForeverOn, DateTime OnUntil);
}
