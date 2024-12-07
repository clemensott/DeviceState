using System;

namespace DeviceStateWeb.Models
{
    public record DeviceState(string Id, string Name, DeviceOnState OnState, DeviceActorOnline ActorOnlineState, TransformedValue Value);
}
