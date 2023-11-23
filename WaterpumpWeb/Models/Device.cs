using System;

namespace WaterpumpWeb.Models
{
    public record Device(string Name, bool IsForeverOn, DateTime OnUntil, TimeSpan LastValuesSpan, string ValueName);
}
