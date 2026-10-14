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

        // Latest sensor reading with field name for frontend
        public Dictionary<string, float>? LastReading { get; set; }

        // Room assignment. A device may be unassigned (null). At most one room at a time.
        public Guid? RoomId { get; set; }
        public Room? Room { get; set; }

        // Relations
        public DeviceRelayState? RelayState { get; set; } // For power strip
        public ICollection<SensorReading> SensorReadings { get; set; } = []; // For sensor
    }
}
