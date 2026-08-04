using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Device
{
    public record RegisterDeviceRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Wrong MAC")]
        public required string DeviceId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Wrong deviceSecret")]
        public required string DeviceSecret { get; set; }
    }
}
