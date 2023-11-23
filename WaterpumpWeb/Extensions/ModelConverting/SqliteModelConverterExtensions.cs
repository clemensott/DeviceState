using System;
using System.Data;
using WaterpumpWeb.Models;

namespace WaterpumpWeb.Extensions.ModelConverting
{
    static class SqliteModelConverterExtensions
    {
        public static Device GetDevice(this IDataRecord record)
        {
            string name = GetStringValue(record, "name");
            bool isForeverOn = GetBooleanValue(record, "is_forever_on");
            DateTime onUntil = GetDateTimeValue(record, "is_on_until");
            TimeSpan lastValuesSpan = GetTimeSpanValue(record, "last_values_span");
            string valueName = GetStringValue(record, "value_name");

            return new Device(name, isForeverOn, onUntil, lastValuesSpan, valueName);
        }

        public static DeviceMeasurement GetDeviceMeasurement(this IDataRecord record)
        {
            return new DeviceMeasurement()
            {
                DeviceId = GetStringValue(record, "device_id"),
                ErrorCount = (int?)GetLongValue(record, "error_count"),
                State = (int?)GetLongValue(record, "state"),
                Value = GetDoubleValue(record, "value"),
                Created = GetDateTimeValue(record, "created"),
            };
        }

        private static string GetStringValue(IDataRecord data, string name)
        {
            return data.GetString(data.GetOrdinal(name));
        }

        private static long? GetLongValue(IDataRecord data, string name)
        {
            object value = data.GetValue(data.GetOrdinal(name));

            return value is long ? (long)value : null;
        }

        private static double? GetDoubleValue(IDataRecord data, string name)
        {
            object value = data.GetValue(data.GetOrdinal(name));

            return value as double?;
        }

        private static DateTime GetDateTimeValue(IDataRecord data, string name)
        {
            return DateTime.Parse(data.GetString(data.GetOrdinal(name)));
        }

        private static TimeSpan GetTimeSpanValue(IDataRecord data, string name)
        {
            return TimeSpan.Parse(data.GetString(data.GetOrdinal(name)));
        }

        private static bool GetBooleanValue(IDataRecord data, string name)
        {
            return data.GetInt64(data.GetOrdinal(name)) != 0;
        }
    }
}
