using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WaterpumpWeb.Services.Devices
{
    public class DevicesEvents : IDevicesEvents
    {
        private readonly IDictionary<string, TaskCompletionSource> waits = new Dictionary<string, TaskCompletionSource>();

        public void TriggerStateChange(string deviceId)
        {
            if (waits.TryGetValue(deviceId, out TaskCompletionSource source))
            {
                source.SetResult();
                waits.Remove(deviceId);
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
    }
}
