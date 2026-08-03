using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Model.Entities
{
    public class Device
    {
        public Guid Id { get; init; }
        public string DeviceId { get; init; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SecretToken { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeenAt { get; set; }
    
        // TODO: Add containers for devices
    }
}
