using System;

namespace WaterpumpWeb.Models
{
    public record DeviceState(string Id, string Name, DeviceOnState OnState, DeviceActorOnline ActoOnlineState, TransformedValue Value);
}
