using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Device
{
    public record RegisterDeviceRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Wrong MAC format")]
        public required string DeviceId { get; set; }
        public string? Name { get; set; }
    }
}
