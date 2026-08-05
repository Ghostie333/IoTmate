using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Model.Entities
{
    public class SensorReading
    {
        public long Id { get; set; }
        public Guid DeviceId { get; set; }
        public Device Device { get; set; } = null!;
        public Dictionary<string, float> Data { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}
