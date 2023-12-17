using Microsoft.Extensions.DependencyInjection;
using DeviceStateWeb.Services.Devices;
using DeviceStateWeb.Services.ValueTransformers;

namespace DeviceStateWeb.Extensions.DependencyInjection
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
