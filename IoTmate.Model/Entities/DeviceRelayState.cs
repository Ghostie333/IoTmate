using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IoTmate.Model.Entities
{
    public class DeviceRelayState
    {
        public Guid DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        // Dictionary contain outlets state
        public Dictionary<string, bool> Outlets { get; set; } = new();
    }
}
