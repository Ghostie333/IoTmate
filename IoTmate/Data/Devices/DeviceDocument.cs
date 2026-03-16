using MongoDB.Bson.Serialization.Attributes;

namespace IoTmate.Data.Devices;

public sealed class DeviceDocument
{
    [BsonId]
    public string Id { get; set; } = default!;

    public string OwnerUserId { get; set; } = default!;

    public string Name { get; set; } = default!;

    public bool Online { get; set; }

    public DateTimeOffset? LastSeenUtc { get; set; }
}
