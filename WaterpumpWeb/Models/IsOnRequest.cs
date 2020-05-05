using System;
using System.Data;

namespace WaterpumpWeb.Models
{
    public struct IsOnRequest
    {
        public int Id { get; private set; }

        public int? DeviceId { get; private set; }

        public int? ErrorCount { get; private set; }

        public int? PumpState { get; private set; }

        public int? RawTemp { get; private set; }

        public bool Response { get; private set; }

        public DateTime Created { get; private set; }

        public IsOnRequest(int? deviceId, int? errorCount, int? pumpState, int? rawTemp, bool response) : this()
        {
            Created = DateTime.Now;
            DeviceId = deviceId;
            ErrorCount = errorCount;
            PumpState = pumpState;
            RawTemp = rawTemp;
            Response = response;
        }

        public static IsOnRequest FromDataRecord(IDataRecord record)
        {
            return new IsOnRequest()
            {
                Id = GetIntValue(record, "id") ?? -1,
                DeviceId = GetIntValue(record, "device_id"),
                ErrorCount = GetIntValue(record, "error_count"),
                PumpState = GetIntValue(record, "pump_state"),
                RawTemp = GetIntValue(record, "raw_temp"),
                Response = GetBooleanValue(record, "response"),
                Created = GetDateTimeValue(record, "created"),
            };
        }

        private static int? GetIntValue(IDataRecord data, string name)
        {
            object value = data.GetValue(data.GetOrdinal(name));

            return value is long ? (int?)(long?)value : null;
        }

        private static DateTime GetDateTimeValue(IDataRecord data, string name)
        {
            return DateTime.Parse(data.GetString(data.GetOrdinal(name)));
        }

        private static bool GetBooleanValue(IDataRecord data, string name)
        {
            return data.GetInt64(data.GetOrdinal(name)) != 0;
        }
    }
}
