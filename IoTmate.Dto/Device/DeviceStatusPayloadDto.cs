using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace IoTmate.Dto.Device
{
    public record DeviceStatusPayloadDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("isOnline")]
        public bool IsOnline { get; set; }
    }
}
