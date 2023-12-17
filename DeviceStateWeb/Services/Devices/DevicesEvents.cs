using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceStateWeb.Models;

namespace DeviceStateWeb.Services.Devices
{
    public class DevicesEvents : IDevicesEvents
    {
        private readonly IDictionary<string, TaskCompletionSource> waits = new Dictionary<string, TaskCompletionSource>();
        private readonly IDictionary<string, TaskCompletionSource> actorWaits = new Dictionary<string, TaskCompletionSource>();
        private readonly IDictionary<string, DateTime> lastActorUpdates = new Dictionary<string, DateTime>();

        public void TriggerStateChange(string deviceId)
        {
            TaskCompletionSource source;
            if (waits.TryGetValue(deviceId, out source))
            {
                source.SetResult();
                waits.Remove(deviceId);
            }

            if (actorWaits.TryGetValue(deviceId, out source))
            {
                source.SetResult();
                actorWaits.Remove(deviceId);
            }
        }

        public async Task<bool> WaitForStateChange(string deviceId, TimeSpan maxWaitTime)
        {
            if (maxWaitTime < TimeSpan.Zero) return false;

            if (!waits.TryGetValue(deviceId, out TaskCompletionSource source))
            {
                source = new TaskCompletionSource();
                waits.Add(deviceId, source);
            }

            Task task = source.Task;
            await Task.WhenAny(task, Task.Delay(maxWaitTime));
            return task.IsCompleted;
        }

        public async Task<bool> ActorWaitForStateChange(string deviceId, TimeSpan maxWaitTime)
        {
            lastActorUpdates[deviceId] = DateTime.UtcNow;
            TriggerStateChange(deviceId);

            if (maxWaitTime < TimeSpan.Zero) return false;

            if (!actorWaits.TryGetValue(deviceId, out TaskCompletionSource source))
            {
                source = new TaskCompletionSource();
                actorWaits.Add(deviceId, source);
            }

            Task task = source.Task;
            await Task.WhenAny(task, Task.Delay(maxWaitTime));

            actorWaits.Remove(deviceId);
            lastActorUpdates[deviceId] = DateTime.UtcNow;

            return task.IsCompleted;
        }

        public DeviceActorOnline GetLastActorUpdate(string deviceId)
        {
            if (actorWaits.ContainsKey(deviceId)) return new DeviceActorOnline(true, DateTime.UtcNow);

            return lastActorUpdates.TryGetValue(deviceId, out DateTime lastUpdate)
                ? new DeviceActorOnline(false, lastUpdate) : new DeviceActorOnline(false, null);
        }
    }
}
