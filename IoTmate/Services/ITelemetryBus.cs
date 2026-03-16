using IoTmate.Contracts.Telemetry;

namespace IoTmate.Services;

public interface ITelemetryBus
{
    void Publish(TelemetryPointDto point);
    IReadOnlyList<TelemetryPointDto> GetLatest(string deviceId, int take);
}
