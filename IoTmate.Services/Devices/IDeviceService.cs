using IoTmate.Dto.Device;
using IoTmate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Services.Devices
{
    public interface IDeviceService
    {
        Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken, CancellationToken ct);
        bool IsTopicAllowedForDevice(string deviceId, string topic);
        Task<DeviceRegistrationResult?> RegisterDeviceAsync(RegisterDeviceRequestDto request, CancellationToken ct);
        Task UpdateDeviceAsync(string deviceId, string newName, string currentUserId, CancellationToken ct);
    }
}
