using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace WaterpumpWeb.Services.Database
{
    public interface ISqlExecuteService
    {
        Task<int> ExecuteNonQueryAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<T> ExecuteScalarAsync<T>(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<object> ExecuteScalarAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<IDataRecord> ExecuteReadFirstAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);

        Task<IEnumerable<IDataRecord>> ExecuteReadAllAsync(string sql, IEnumerable<KeyValuePair<string, object>> parameters = null);
    }
}
