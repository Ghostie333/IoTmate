using System.Collections.Generic;

namespace IoTmate.Dto.DeviceData
{
    public record DeviceStateUpdateDto
    {
        public required string DeviceId { get; set; }
        public required bool IsOnline { get; set; }
        public required DateTime LastSeenAt { get; set; }

        public Dictionary<string, bool> Outlets { get; set; } = [];
    }
}
