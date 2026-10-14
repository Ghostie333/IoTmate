using IoTmate.Dto.Device;
using IoTmate.Dto.DeviceData;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using IoTmate.Repository.Devices;
using IoTmate.Services.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IoTmate.Services.Devices.StateHandlers
{
    public class PowerStripStateHandler(
        ILogger<PowerStripStateHandler> logger,
        IHubContext<DeviceHub, IDeviceHubClient> deviceHub) : IDeviceStateHandler
    {
        public DeviceType DeviceType => DeviceType.PowerStrip;

        private readonly ILogger<PowerStripStateHandler> _logger = logger;
        private readonly IHubContext<DeviceHub, IDeviceHubClient> _deviceHub = deviceHub;

        public async Task HandleStatePayloadAsync(Device device, string payload, IServiceScope scope, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(payload))
                return;

            try
            {
                var outletsState = JsonSerializer.Deserialize<bool[]>(payload);
                if (outletsState == null || outletsState.Length == 0)
                    return;

                device.RelayState ??= new DeviceRelayState { DeviceId = device.Id };
                device.RelayState.OutletsState = outletsState;
                device.LastSeenAt = DateTime.UtcNow;

                var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
                bool success = await deviceRepository.SaveDeviceAsync(device, ct);

                if (device.RoomId is not null && success)
                {
                    await _deviceHub.Clients.Group(device.RoomId.Value.ToString())
                        .DeviceStateChanged(new DeviceStateUpdateDto
                        {
                            DeviceId = device.DeviceId,
                            IsOnline = device.IsOnline,
                            LastSeenAt = device.LastSeenAt,
                            OutletsState = outletsState
                        });
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "[PowerStripStateHandler] Error deserializing state payload for device {DeviceId}", device.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PowerStripStateHandler] Unexpected error while handling state for device {DeviceId}", device.DeviceId);
            }
        }
    }
}
