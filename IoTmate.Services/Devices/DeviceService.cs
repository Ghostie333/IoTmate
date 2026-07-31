using IoTmate.Model.Entities;
using IoTmate.Repository.Devices;
using System.Security.Cryptography;
using System.Text;

namespace IoTmate.Services.Devices
{
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

        public async Task<string?> RegisterDeviceAsync(string deviceId, string? name)
        {
            var normalizedMac = deviceId.Trim().ToUpperInvariant();

            var existingDevice = await _deviceRepository.GetByDeviceIdAsync(normalizedMac);
            if (existingDevice != null)
            {
                return null; // Device exists
            }

            string rawSecretToken = GenerateSecureToken();
            string tokenHash = HashToken(rawSecretToken);

            var newDevice = new Device
            {
                DeviceId = normalizedMac,
                Name = name ?? $"ESP32-{normalizedMac}",
                SecretToken = tokenHash,
            };

            bool saved = await _deviceRepository.SaveDeviceAsync(newDevice);
            return saved ? rawSecretToken : null;
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToHexString(randomBytes);
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}
