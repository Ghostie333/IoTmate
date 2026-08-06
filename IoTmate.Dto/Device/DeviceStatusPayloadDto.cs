using System.Text.Json.Serialization;

namespace IoTmate.Dto.Device
{
    public record DeviceStatusPayloadDto
    {
        // The device publishes {"online":true|false} on the status topic.
        [JsonPropertyName("online")]
        public bool IsOnline { get; set; }
    }
}