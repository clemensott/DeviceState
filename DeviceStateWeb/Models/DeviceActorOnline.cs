using System;

namespace DeviceStateWeb.Models
{
    public record DeviceActorOnline(bool IsOnline, DateTime? LastUpdate);
}
