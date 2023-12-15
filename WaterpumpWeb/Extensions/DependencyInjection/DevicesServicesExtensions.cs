using Microsoft.Extensions.DependencyInjection;
using WaterpumpWeb.Services.Devices;
using WaterpumpWeb.Services.ValueTransformers;

namespace WaterpumpWeb.Extensions.DependencyInjection
{
    static class DevicesServicesExtensions
    {
        public static void AddDevicesServices(this IServiceCollection services)
        {
            services.AddSingleton<IDevicesEvents, DevicesEvents>();
            services.AddScoped<IValueTransformer, LinearPointValueTransformer>();
            services.AddScoped<IDevicesService, DevicesService>();
        }
    }
}
