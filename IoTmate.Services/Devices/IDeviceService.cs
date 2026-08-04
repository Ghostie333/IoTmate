using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Services.Devices
{
    public interface IDeviceService
    {
        Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken);
        bool IsTopicAllowedForDevice(string deviceId, string topic);
        Task<string?> RegisterDeviceAsync(string deviceId, string deviceSecret);
    }
}
