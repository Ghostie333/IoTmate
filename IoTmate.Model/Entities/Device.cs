using IoTmate.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Model.Entities
{
    public class Device
    {
        public Guid Id { get; set; }
        public required string DeviceId { get; set; } // MAC
        public required string DeviceSecret { get; set; }
        public required string SecretToken { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceType DeviceType { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeenAt { get; set; }

        // Relations
        public DeviceRelayState? RelayState { get; set; } // For StripBoard
        public ICollection<SensorReading> SensorReadings { get; set; } = []; // For sensor
    }
}
