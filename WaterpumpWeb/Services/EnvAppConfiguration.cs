using System;

namespace WaterpumpWeb.Services
{
    public class EnvAppConfiguration : IAppConfiguration
    {
        public string DatabaseConnectionString { get; }
        
        public string DefaultDeviceId { get; }

        public EnvAppConfiguration()
        {
            DatabaseConnectionString = Environment.GetEnvironmentVariable("DEVICES_DB_CONNECTION_STRING");
            DefaultDeviceId = Environment.GetEnvironmentVariable("DEVICES_DEFAULT_DEVICE_ID");
        }
    }
}
