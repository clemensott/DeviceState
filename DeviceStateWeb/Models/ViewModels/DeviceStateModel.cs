namespace DeviceStateWeb.Models.ViewModels
{
    public record DeviceStateModel(string Id, string Name, DeviceOnState OnState, DeviceActorOnline ActorOnline, TransformedValue Value);
}
