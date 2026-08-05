using IoTmate.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace IoTmate.Dto.Device
{
    public record RegisterDeviceRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Wrong MAC")]
        public required string DeviceId { get; init; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Wrong deviceSecret")]
        public required string DeviceSecret { get; init; }

        [Required(ErrorMessage = "Device type is required")]
        public required DeviceType DeviceType { get; init; }

        // Only for PowerStrip
        [Range(1, 16, ErrorMessage = "Outlet count must be 1..16")]
        public int? OutletsCount { get; init; }
    }
}
