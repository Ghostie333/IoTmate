using IoTmate.Common.Exceptions;
using IoTmate.Common.Security;
using IoTmate.Dto.Device;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using IoTmate.Repository.Devices;
using IoTmate.Services.Rooms;
using Microsoft.AspNetCore.Identity;

namespace IoTmate.Services.Devices;

public class DeviceService(IDeviceRepository deviceRepository, IRoomService roomService) : IDeviceService
{
    private readonly PasswordHasher<Device> _passwordHasher = new();

    public async Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken, CancellationToken ct)
    {
        return await deviceRepository.ValidateCredentialsAsync(deviceId, secretToken, ct);
    }

    public bool IsTopicAllowedForDevice(string deviceId, string topic)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(topic))
        {
            return false;
        }

        var normalizedMac = deviceId.Trim().ToUpperInvariant();
        var prefix = $"devices/{normalizedMac}";

        // ACL rule - device is only granted access to devices/{MAC} and its subtopics (devices/{MAC}/*).
        return topic.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
               topic.StartsWith($"{prefix}/", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<DeviceRegistrationResult?> RegisterDeviceAsync(RegisterDeviceRequestDto request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.DeviceId) || string.IsNullOrWhiteSpace(request.DeviceSecret))
        {
            return null;
        }

        var normalizedMac = request.DeviceId.Trim().ToUpperInvariant();
        var existingDevice = await deviceRepository.GetByDeviceIdAsync(normalizedMac, ct);

        // If the device already exists
        if (existingDevice != null)
        {
            // Verify the hashed DeviceSecret
            var verificationResult = _passwordHasher.VerifyHashedPassword(
                existingDevice,
                existingDevice.DeviceSecret,
                request.DeviceSecret
            );

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null; // Invalid DeviceSecret
            }

            // Refresh the MQTT session token using the static CryptoUtils
            string newRawToken = CryptoUtils.GenerateSecureToken();
            existingDevice.SecretToken = CryptoUtils.HashTokenToHex(newRawToken);

            await deviceRepository.SaveDeviceAsync(existingDevice, ct);
            return new DeviceRegistrationResult { Device = existingDevice, RawSecretToken = newRawToken };
        }

        // If the device is being created for the first time
        string rawSecretToken = CryptoUtils.GenerateSecureToken();
        string tokenHash = CryptoUtils.HashTokenToHex(rawSecretToken);

        var newDevice = new Device
        {
            DeviceId = normalizedMac,
            Name = $"ESP32-{normalizedMac}",
            DeviceSecret = "",
            SecretToken = tokenHash,
            DeviceType = request.DeviceType
        };

        // Hash the DeviceSecret received from the ESP32
        newDevice.DeviceSecret = _passwordHasher.HashPassword(newDevice, request.DeviceSecret);

        if (request.DeviceType == DeviceType.PowerStrip)
        {
            newDevice.RelayState = new DeviceRelayState
            {
                OutletsState = CreateInitialOutletsState(request.OutletsCount ?? 1),
                OutletsNames = CreateInitialOutletsNames(request.OutletsCount ?? 1)
            };
        }

        await deviceRepository.SaveDeviceAsync(newDevice, ct);
        return new DeviceRegistrationResult { Device = newDevice, RawSecretToken = rawSecretToken };
    }

    public async Task UpdateDeviceAsync(string deviceId, string newName, string currentUserId, CancellationToken ct)
    {
        var normalizedMac = deviceId.Trim().ToUpperInvariant();
        var device = await deviceRepository.GetByDeviceIdAsync(normalizedMac, ct)
            ?? throw new NotFoundException("device.notFound");

        if (device.RoomId is null)
            throw new NotFoundException("device.notFound");

        await roomService.GetUserRole(device.RoomId.Value, currentUserId, ct);

        device.Name = newName;
        await deviceRepository.SaveDeviceAsync(device, ct);
    }

    private static bool[] CreateInitialOutletsState(int count)
    {
        return new bool[count];
    }

    private static string[] CreateInitialOutletsNames(int count)
    {
        var names = new string[count];
        for (int i = 0; i < count; i++)
        {
            names[i] = $"outlet{i + 1}";
        }
        return names;
    }
}