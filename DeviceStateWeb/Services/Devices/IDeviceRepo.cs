﻿using System;
using System.Threading.Tasks;
using DeviceStateWeb.Models;

namespace DeviceStateWeb.Services.Devices
{
    public interface IDeviceRepo
    {
        Task<Device> GetDevice(string id);

        Task<bool> SetDeviceOnState(string id, DeviceDesiredOnState onState);

        Task SetLastActorUpdate(string id, DateTime lastActorUpdate);

        Task<DeviceMeasurement[]> GetMeasurements(string id, TimeSpan last);

        Task SetMeasurement(DeviceMeasurement measurement);
    }
}
