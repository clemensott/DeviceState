using System;

namespace WaterpumpWeb.Services
{
    public class EnvAppConfiguration : IAppConfiguration
    {
        public string DatabaseConnectionString { get; }

        public string DefaultDeviceId { get; }

        public TimeSpan ActorOnlineTolerance { get; }

        public EnvAppConfiguration()
        {
            DatabaseConnectionString = Environment.GetEnvironmentVariable("DEVICES_DB_CONNECTION_STRING");
            DefaultDeviceId = Environment.GetEnvironmentVariable("DEVICES_DEFAULT_DEVICE_ID");

            string actorOnlineToleranceString = Environment.GetEnvironmentVariable("DEVICES_ACTOR_ONLINE_TOLERANCE");
            ActorOnlineTolerance = actorOnlineToleranceString != null
                && TimeSpan.TryParse(actorOnlineToleranceString, out TimeSpan actorOnlineTolerance)
                ? actorOnlineTolerance : TimeSpan.FromSeconds(3);
        }
    }
}
