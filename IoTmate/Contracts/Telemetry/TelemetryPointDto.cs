namespace IoTmate.Contracts.Telemetry;

public sealed record TelemetryPointDto(
    string DeviceId,
    string Key,
    double Value,
    DateTimeOffset TimestampUtc
);
