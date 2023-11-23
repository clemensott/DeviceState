using Microsoft.Extensions.DependencyInjection;
using WaterpumpWeb.Services.Devices;

namespace WaterpumpWeb.Extensions.DependencyInjection
{
    static class DevicesServicesExtensions
    {
        public static void AddDevicesServices(this IServiceCollection services)
        {
            services.AddSingleton<IDevicesEvents, DevicesEvents>();
            services.AddScoped<IDevicesService, DevicesService>();
        }
    }
}
