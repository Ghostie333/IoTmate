using IoTmate.Dto.Device;
using IoTmate.Dto.DeviceData;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using IoTmate.Repository.Devices;
using IoTmate.Services.Hubs;
using IoTmate.Services.Mqtt;
using IoTmate.Services.Rules;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace IoTmate.Services.Devices.StateHandlers
{
    public sealed class SensorStateHandler(
        IDeviceRepository deviceRepository,
        IHubContext<DeviceHub, IDeviceHubClient> deviceHub) : IDeviceStateHandler
    {
        public DeviceType DeviceType => DeviceType.Sensor;

        public async Task HandleStatePayloadAsync(Device device, string payload, IServiceScope scope, CancellationToken ct)
        {
            var readings = JsonSerializer.Deserialize<Dictionary<string, float>>(payload);
            if (readings == null || readings.Count == 0)
                return;

            device.LastReading = readings;
            device.SensorReadings.Add(new SensorReading { DeviceId = device.Id, Data = readings });
            device.LastSeenAt = DateTime.UtcNow;

            bool success = await deviceRepository.SaveDeviceAsync(device, ct);

            // Get rules from scope
            var ruleEngine = scope.ServiceProvider.GetRequiredService<IRuleEngineService>();
            await ruleEngine.HandleReadingRuleAsync(device.Id, readings, ct);

            if (device.RoomId is not null && success)
            {
                await deviceHub.Clients.Group(device.RoomId.Value.ToString())
                    .SensorReadingReceived(new SensorReadingUpdateDto
                    {
                        DeviceId = device.DeviceId,
                        Data = readings,
                        Timestamp = device.LastSeenAt
                    });
            }
        }
    }
}
