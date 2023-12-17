﻿using System;
using System.Threading.Tasks;
using DeviceStateWeb.Models;

namespace DeviceStateWeb.Services.Devices
{
    public interface IDevicesEvents
    {
        Task<bool> WaitForStateChange(string deviceId, TimeSpan maxWaitTime);

        /// <summary>
        /// Waits for stage change trigger.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="maxWaitTime"></param>
        /// <returns>Returns true if state changed and false if function ran into the timeout</returns>
        Task<bool> ActorWaitForStateChange(string deviceId, TimeSpan maxWaitTime);

        DeviceActorOnline GetLastActorUpdate(string deviceId);

        void TriggerStateChange(string deviceId);
    }
}
