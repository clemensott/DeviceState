using Microsoft.Extensions.DependencyInjection;
using WaterpumpWeb.Services.Database;
using WaterpumpWeb.Services.Devices.Sql;
using WaterpumpWeb.Services.Devices;
using WaterpumpWeb.Services.Database.Sqlite;

namespace WaterpumpWeb.Extensions.DependencyInjection
{
    static class SqlServicesExtensions
    {
        public static void AddSqlServices(this IServiceCollection services)
        {
            services.AddSingleton<ISqlExecuteService, SqliteExecuteService>();
            services.AddScoped<IDeviceRepo, SqlDeviceRepo>();
        }
    }
}
