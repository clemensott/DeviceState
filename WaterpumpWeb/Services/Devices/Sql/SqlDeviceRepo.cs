using System.Data;
using System;
using WaterpumpWeb.Models;
using WaterpumpWeb.Services.Database;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WaterpumpWeb.Extensions.ModelConverting;

namespace WaterpumpWeb.Services.Devices.Sql
{
    public class SqlDeviceRepo : IDeviceRepo
    {
        private readonly ISqlExecuteService sqlExecuteService;

        public SqlDeviceRepo(ISqlExecuteService sqlExecuteService)
        {
            this.sqlExecuteService = sqlExecuteService;
        }

        public async Task SetMeasurement(DeviceMeasurement measurement)
        {
            const string insertSql = @"
                INSERT INTO device_measurements (device_id, error_count, state, value, created)
                VALUES (@deviceId, @errorCount, @state, @value, @created);
            ";

            KeyValuePair<string, object>[] insertParameters = new KeyValuePair<string, object>[]{
                new KeyValuePair<string, object>("@deviceId", measurement.DeviceId),
                new KeyValuePair<string, object>("@errorCount", measurement.ErrorCount),
                new KeyValuePair<string, object>("@state", measurement.State),
                new KeyValuePair<string, object>("@value", measurement.Value),
                new KeyValuePair<string, object>("@created", measurement.Created),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(insertSql, insertParameters);

            const string deleteSql = @"
                DELETE
                FROM device_measurements
                WHERE device_id = @id AND id NOT IN (SELECT id FROM device_measurements ORDER BY created DESC LIMIT 10000);
            ";
            KeyValuePair<string, object>[] deleteParameters = new[]
            {
                new KeyValuePair<string, object>("id", measurement.DeviceId),
            };

            await sqlExecuteService.ExecuteNonQueryAsync(deleteSql, deleteParameters);
        }

        public async Task<DeviceMeasurement[]> GetMeasurements(string id, TimeSpan last)
        {
            const string sql = @"
                SELECT device_id, error_count, state, value, created
                FROM device_measurements
                WHERE device_id = @id AND created >= @minDate
                ORDER BY created DESC;
            ";
            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>("@id", id),
                new KeyValuePair<string, object>("@minDate", DateTime.UtcNow - last),
            };

            IEnumerable<IDataRecord> records = await sqlExecuteService.ExecuteReadAllAsync(sql, parameters);
            return records.Select(SqliteModelConverterExtensions.GetDeviceMeasurement).ToArray();
        }

        public async Task<Device> GetDevice(string id)
        {
            const string sql = "SELECT name, is_forever_on, is_on_until, last_values_span, value_name FROM devices WHERE id = @id LIMIT 1";
            KeyValuePair<string, object>[] parameters = new[]
            {
                new KeyValuePair<string, object>("@id", id),
            };

            IDataRecord data = await sqlExecuteService.ExecuteReadFirstAsync(sql, parameters);
            return data?.GetDevice();
        }

        public async Task<bool> SetDeviceOnState(string id, DeviceOnState state)
        {
            const string sql = "UPDATE devices SET is_forever_on = @isForeverOn, is_on_until = @isOnUntil WHERE id = @id;";

            KeyValuePair<string, object>[] parameters = new[]
            {
                new KeyValuePair<string, object>("@isForeverOn", state.IsForeverOn),
                new KeyValuePair<string, object>("@isOnUntil", state.OnUntil),
                new KeyValuePair<string, object>("@id", id),
            };

            int count = await sqlExecuteService.ExecuteNonQueryAsync(sql, parameters);
            return count > 0;
        }
    }
}
