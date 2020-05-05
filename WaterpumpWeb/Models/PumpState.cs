using System;

namespace WaterpumpWeb.Models
{
    public struct PumpState
    {
        public bool? IsOn { get; }

        public double RemainingOnMillis { get; }

        public double? LastUpdateMillisAgo { get; }

        public Temperature Temperature { get; }

        public PumpState(bool? isOn, double remainingOnMillis, double? lastUpdateMillisAgo, Temperature temperature) : this()
        {
            IsOn = isOn;
            RemainingOnMillis = remainingOnMillis;
            LastUpdateMillisAgo = lastUpdateMillisAgo;
            Temperature = temperature;
        }
    }
}
