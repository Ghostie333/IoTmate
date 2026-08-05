using IoTmate.Services.Devices;
using Microsoft.Extensions.Logging;

namespace IoTmate.Services.Mqtt
{
    public class MqttMessageRouterService(ILogger<MqttMessageRouterService> logger, IDeviceStateService deviceStateService) : IMqttMessageRouterService
    {
        private readonly ILogger<MqttMessageRouterService> _logger = logger;
        private readonly IDeviceStateService _deviceStateService = deviceStateService;

        public async Task HandleAsync(string topic, string payload, CancellationToken ct)
        {
            // devices/{deviceId}/status | state | command
            var segments = topic.Split('/');
            if (segments.Length != 3 || segments[0] != "devices")
            {
                _logger.LogWarning("Unrecognized topic format: {Topic}", topic);
                return;
            }

            var deviceId = segments[1];
            var messageType = segments[2];

            switch (messageType)
            {
                case "status":
                    await _deviceStateService.HandleStatusAsync(deviceId, payload, ct);
                    break;

                case "state":
                    await _deviceStateService.HandleStateAsync(deviceId, payload, ct);
                    break;

                default:
                    _logger.LogWarning("Unhandled message type '{Type}' for device {DeviceId}", messageType, deviceId);
                    break;
            }
        }
    }
}