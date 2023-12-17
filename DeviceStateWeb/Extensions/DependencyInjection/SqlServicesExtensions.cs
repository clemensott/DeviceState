using Microsoft.Extensions.DependencyInjection;
using DeviceStateWeb.Services.Database;
using DeviceStateWeb.Services.Devices.Sql;
using DeviceStateWeb.Services.Devices;
using DeviceStateWeb.Services.Database.Sqlite;

namespace DeviceStateWeb.Extensions.DependencyInjection
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
