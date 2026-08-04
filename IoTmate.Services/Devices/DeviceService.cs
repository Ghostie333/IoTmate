using System.Security.Cryptography;
using System.Text;
using IoTmate.Model.Entities;
using IoTmate.Repository.Devices;

namespace IoTmate.Services.Devices;

public class DeviceService(IDeviceRepository deviceRepository) : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository = deviceRepository;

    public async Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken)
    {
        return await _deviceRepository.ValidateCredentialsAsync(deviceId, secretToken);
    }

    public bool IsTopicAllowedForDevice(string deviceId, string topic)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(topic))
        {
            return false;
        }

        var normalizedMac = deviceId.Trim().ToUpperInvariant();

        // ACL rule - device is only granted access to devices/{MAC} and its subtopics (devices/{MAC}/*).
        return topic.StartsWith($"devices/{normalizedMac}/", StringComparison.OrdinalIgnoreCase) ||
               topic.Equals($"devices/{normalizedMac}", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string?> RegisterDeviceAsync(string deviceId, string deviceSecret)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return null;
        }

        var normalizedMac = deviceId.Trim().ToUpperInvariant();

        var existingDevice = await _deviceRepository.GetByDeviceIdAsync(normalizedMac);
        if (existingDevice != null)
        {
            // Check if hardware secret is correct
            if (!string.Equals(existingDevice.DeviceSecret, deviceSecret, StringComparison.Ordinal))
            {
                return null;
            }

            // Set new session token for MQTT
            string newRawToken = GenerateSecureToken();
            existingDevice.SecretToken = HashToken(newRawToken);

            await _deviceRepository.SaveDeviceAsync(existingDevice);
            return newRawToken;
        }

        string rawSecretToken = GenerateSecureToken();
        string tokenHash = HashToken(rawSecretToken);

        var newDevice = new Device
        {
            DeviceId = normalizedMac,
            DeviceSecret = deviceSecret,
            Name = $"ESP32-{normalizedMac}",
            SecretToken = tokenHash,
        };

        bool saved = await _deviceRepository.SaveDeviceAsync(newDevice);
        return saved ? rawSecretToken : null;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToHexStringLower(randomBytes); // Lowercase dla spójności z C++ / Mosquitto
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}