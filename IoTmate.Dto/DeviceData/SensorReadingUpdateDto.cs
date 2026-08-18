using System;
using System.Collections.Generic;

namespace IoTmate.Dto.DeviceData
{
    public record SensorReadingUpdateDto
    {
        public required string DeviceId { get; set; }
        public Dictionary<string, float> Data { get; set; } = [];
        public DateTime Timestamp { get; set; }
    }
}
