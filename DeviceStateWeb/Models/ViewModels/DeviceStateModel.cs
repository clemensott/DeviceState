using System;

namespace DeviceStateWeb.Models.ViewModels
{
    public record DeviceStateModel(string Id, string Name, DeviceOnState OnState, DeviceActorOnline ActoOnline, TransformedValue Value);
}
