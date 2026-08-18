using System;
using System.Collections.Generic;

namespace IoTmate.Dto.DeviceData
{
    public record DeviceStatusUpdateDto
    {
        public required string DeviceId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeenAt { get; set; }
    }
}
