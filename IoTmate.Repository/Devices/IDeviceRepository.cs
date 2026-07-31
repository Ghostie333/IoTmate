using IoTmate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Repository.Devices
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByDeviceIdAsync(string deviceId);
        Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken);
        Task<bool> SaveDeviceAsync(Device device);
    }
}
