using System.Text.Json;
using IoTmate.Dto.Device;
using IoTmate.Repository.Devices;
using IoTmate.Services.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IoTmate.Services.Devices
{
    public class DeviceStateService(
        MqttBrokerListenerService mqttService,
        IServiceScopeFactory scopeFactory,
        ILogger<DeviceStateService> logger) : IDeviceStateService
    {
        private readonly MqttBrokerListenerService _mqttService = mqttService;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<DeviceStateService> _logger = logger;

        public async Task HandleStateAsync(string deviceId, string payload, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task HandleStatusAsync(string deviceId, string payload, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(payload))
                return;

            try
            {
                // Create scope for repo
                using var scope = _scopeFactory.CreateScope();
                var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

                var device = await deviceRepository.GetByDeviceIdAsync(deviceId);
                if (device == null)
                    return;

                JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
                var statusData = JsonSerializer.Deserialize<DeviceStatusPayloadDto>(payload, options);

                if (statusData == null)
                    return;

                device.IsOnline = statusData.IsOnline;
                device.LastSeenAt = DateTime.UtcNow;

                await deviceRepository.SaveDeviceAsync(device);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "[DeviceStateService] Error deserializing DeviceStatusPayload");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeviceStateService] Unexpected error while handling status for device {DeviceId}", deviceId);
            }
        }

        public async Task SendRelayCommandAsync(string deviceId, int relayNum, bool turnOn, CancellationToken ct)
        {
            var topic = $"devices/{deviceId}/command";
            var payload = $"{(turnOn ? "ON" : "OFF")}_{relayNum}";
            await _mqttService.PublishAsync(topic, payload, ct);
        }
    }
}