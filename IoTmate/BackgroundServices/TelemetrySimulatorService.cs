using IoTmate.Contracts.Devices;
using IoTmate.Contracts.Telemetry;
using IoTmate.Services;

namespace IoTmate.BackgroundServices;

public sealed class TelemetrySimulatorService : BackgroundService
{
    private readonly IDeviceStore _devices;
    private readonly ITelemetryBus _telemetry;

    private const string SystemOwnerUserId = "system";

    public TelemetrySimulatorService(IDeviceStore devices, ITelemetryBus telemetry)
    {
        _devices = devices;
        _telemetry = telemetry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var device in _devices.GetAll(SystemOwnerUserId))
            {
                var now = DateTimeOffset.UtcNow;

                _devices.Upsert(SystemOwnerUserId, device with { Online = true, LastSeenUtc = now });

                _telemetry.Publish(new TelemetryPointDto(
                    DeviceId: device.Id,
                    Key: "temp",
                    Value: 18 + random.NextDouble() * 10,
                    TimestampUtc: now));

                _telemetry.Publish(new TelemetryPointDto(
                    DeviceId: device.Id,
                    Key: "humidity",
                    Value: 35 + random.NextDouble() * 30,
                    TimestampUtc: now));
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
