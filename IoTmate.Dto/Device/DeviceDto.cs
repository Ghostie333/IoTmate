namespace IoTmate.Dto.Device;
using IoTmate.Model.Enums;

public record DeviceDto
{
    public required string DeviceId { get; set; }
    public required string Name { get; set; }
    public DeviceType DeviceType { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeenAt { get; set; }
    public bool[]? OutletsState { get; set; } // For powerstrip
    public Dictionary<string, float>? LastReading { get; set; } // For sensor
}
