namespace IoTmate.Contracts.Devices;

public sealed record DeviceDto(
    string Id,
    string Name,
    bool Online,
    DateTimeOffset? LastSeenUtc
);
