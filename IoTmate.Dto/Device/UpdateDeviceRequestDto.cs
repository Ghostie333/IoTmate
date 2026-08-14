using System.ComponentModel.DataAnnotations;

namespace IoTmate.Dto.Device
{
    public record UpdateDeviceRequestDto
    {
        [Required]
        public required string NewName { get; set; }
    }
}
