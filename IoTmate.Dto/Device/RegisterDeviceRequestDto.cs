using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Device
{
    public record RegisterDeviceRequestDto
    {
        public string DeviceId { get; set; }
        public string? Name { get; set; }
    }
}
