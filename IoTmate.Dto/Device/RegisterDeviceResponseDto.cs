using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Device
{
    public record RegisterDeviceResponseDto
    {
        public required string DeviceId { get; set; }
        public required string SecretToken { get; set; }
    }
}
