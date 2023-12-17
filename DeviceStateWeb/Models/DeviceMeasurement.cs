using System;

namespace DeviceStateWeb.Models
{
    public struct DeviceMeasurement
    {
        public string DeviceId { get; set; }

        public int? ErrorCount { get; init; }

        public int? State { get; init; }

        public double? Value { get; init; }

        public DateTime Created { get; init; }

        public DeviceMeasurement(string deviceId, int? errorCount, int? state, int? value) : this()
        {
            DeviceId = deviceId;
            Created = DateTime.UtcNow;
            ErrorCount = errorCount;
            State = state;
            Value = value;
        }
    }
}
