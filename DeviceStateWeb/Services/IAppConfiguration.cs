using System;

namespace DeviceStateWeb.Services
{
    public interface IAppConfiguration
    {
        string DatabaseConnectionString { get; }

        /// <summary>
        /// Device ID that gets used when requests don't send one
        /// </summary>
        string DefaultDeviceId { get; }

        /// <summary>
        /// File path to map for linear points map.
        /// </summary>
        string LinearPointsMapPath { get; }

        /// <summary>
        /// Time after last request of actor in which actor is still considered to be "online".
        /// Online means having a long polling request in the going.
        /// </summary>
        TimeSpan ActorOnlineTolerance { get; }
    }
}
