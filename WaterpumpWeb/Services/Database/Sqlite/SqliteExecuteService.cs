using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WaterpumpWeb.Services.Database.Sqlite
{
    public class SqliteExecuteService : ISqlExecuteService
    {
        private readonly string dbConnectionString;
        private SqliteConnection connection;

        public SqliteExecuteService(IAppConfiguration appConfiguration)
        {
            dbConnectionString = appConfiguration.DatabaseConnectionString;
        }

        private async Task<SqliteCommand> GetCommand(string sql, IEnumerable<KeyValuePair<string, object>> parameters)
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
                    command.Parameters.AddWithValue(pair.Key, pair.Value ?? DBNull.Value);
                }
            }

            return command;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using SqliteCommand command = await GetCommand(sql, parameters);
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            return (T)await ExecuteScalarAsync(sql, parameters);
        }

        public async Task<object> ExecuteScalarAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using SqliteCommand command = await GetCommand(sql, parameters);
            return await command.ExecuteScalarAsync();
        }

        public async Task<IDataRecord> ExecuteReadFirstAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using SqliteCommand command = await GetCommand(sql, parameters);
            using SqliteDataReader reader = await command.ExecuteReaderAsync();
            return reader.Cast<IDataRecord>().FirstOrDefault();
        }

        public async Task<IEnumerable<IDataRecord>> ExecuteReadAllAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            using SqliteCommand command = await GetCommand(sql, parameters);
            using SqliteDataReader reader = await command.ExecuteReaderAsync();
            return reader.Cast<IDataRecord>().ToArray();
        }
    }
}
