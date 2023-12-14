using System;

namespace WaterpumpWeb.Models
{
    public record DeviceActorOnline(bool IsOnline, DateTime? LastUpdate);
}
