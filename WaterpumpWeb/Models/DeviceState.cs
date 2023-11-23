using System;

namespace WaterpumpWeb.Models
{
    public readonly struct DeviceState
    {
        public string Id { get; }

        public string Name { get; }

        public bool? IsOn { get; }

        public bool IsForeverOn { get; }

        public DateTime OnUntil { get; }

        public DateTime? LastUpdate { get; }

        public TransformedValue Value { get; }

        public DeviceState(string id, string name, bool? isOn, bool isForeverOn, DateTime onUntil, DateTime? lastUpdate, TransformedValue value) : this()
        {
            Id = id;
            Name = name;
            IsOn = isOn;
            IsForeverOn = isForeverOn;
            OnUntil = onUntil;
            LastUpdate = lastUpdate;
            Value = value;
        }
    }
}
