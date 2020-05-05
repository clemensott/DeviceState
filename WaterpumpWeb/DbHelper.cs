using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WaterpumpWeb
{
    public static class DbHelper
    {
        private const string dbConnectionString = @"Data Source=waterpump.db";
        //private const string dbConnectionString = @"Data Source=Y:\Portable\WaterpumpV2\waterpump.db";

        private static SqliteConnection connection;

        private static async Task<SqliteCommand> GetCommand(string sql, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                connection?.Close();
                connection = new SqliteConnection(dbConnectionString);
                await connection.OpenAsync();
            }

            SqliteCommand command = connection.CreateCommand();

            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> pair in parameters)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            }

            return command;
        }

        public static async Task<int> ExecuteNonQueryAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<object> ExecuteScalarAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                return await command.ExecuteScalarAsync();
            }
        }

        public static async Task<IDataRecord> ExecuteReadFirstAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    return reader.Cast<IDataRecord>().FirstOrDefault();
                }
            }
        }

        public static async Task<IEnumerable<IDataRecord>> ExecuteReadAllAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using (SqliteCommand command = await GetCommand(sql, parameters))
            {
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    return reader.Cast<IDataRecord>().ToArray();
                }
            }
        }
    }
}
