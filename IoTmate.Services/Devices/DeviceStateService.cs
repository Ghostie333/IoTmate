using IoTmate.Dto.Device;
using IoTmate.Dto.DeviceData;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using IoTmate.Repository.Devices;
using IoTmate.Services.Devices.StateHandlers;
using IoTmate.Services.Hubs;
using IoTmate.Services.Mqtt;
using IoTmate.Services.Rules;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IoTmate.Services.Devices
{
    public class DeviceStateService(
        MqttBrokerListenerService mqttService,
        IServiceScopeFactory scopeFactory,
        ILogger<DeviceStateService> logger,
        IHubContext<DeviceHub, IDeviceHubClient> deviceHub) : IDeviceStateService
    {
        private readonly MqttBrokerListenerService _mqttService = mqttService;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<DeviceStateService> _logger = logger;
        private readonly IHubContext<DeviceHub, IDeviceHubClient> _deviceHub = deviceHub;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task HandleStateAsync(string deviceId, string payload, bool retained, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(payload))
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

                var device = await deviceRepository.GetByDeviceIdAsync(deviceId, ct);
                if (device == null)
                    return;

                // Get handler for correct device type
                var handler = scope.ServiceProvider.GetRequiredKeyedService<IDeviceStateHandler>(device.DeviceType);
                await handler.HandleStatePayloadAsync(device, payload, retained, scope, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeviceStateService] Unexpected error while handling state for device {DeviceId}", deviceId);
            }
        }

        public async Task HandleStatusAsync(string deviceId, string payload, bool retained, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(payload))
                return;

            if (retained)
            {
                _logger.LogDebug("[DeviceStateService] Ignoring retained status for device {DeviceId}", deviceId);
                return;
            }

            try
            {
                // Create a scope for the scoped repository
                using var scope = _scopeFactory.CreateScope();
                var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

                var device = await deviceRepository.GetByDeviceIdAsync(deviceId, ct);
                if (device == null)
                    return;

                var statusData = JsonSerializer.Deserialize<DeviceStatusPayloadDto>(payload, JsonOptions);
                if (statusData == null)
                    return;

                device.IsOnline = statusData.IsOnline;
                device.LastSeenAt = DateTime.UtcNow;

                bool success = await deviceRepository.SaveDeviceAsync(device, ct);

                // Notice clients about status update
                if (device.RoomId is not null && success)
                    await _deviceHub.Clients.Group(device.RoomId.Value.ToString())
                        .DeviceStatusChanged(new DeviceStatusUpdateDto
                        {
                            DeviceId = device.DeviceId,
                            IsOnline = device.IsOnline,
                            LastSeenAt = device.LastSeenAt
                        });
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