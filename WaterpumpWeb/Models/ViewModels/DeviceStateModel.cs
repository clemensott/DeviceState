using System;

namespace WaterpumpWeb.Models.ViewModels
{
    public record DeviceStateModel(string Id, string Name, bool? IsOn, bool IsForeverOn, DateTime OnUntil, DateTime? LastUpdate, TransformedValue Value);
}
