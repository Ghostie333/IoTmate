using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Device
{
    public record MqttUserAuthRequestDto
    {
        [FromForm(Name = "username")]
        public string Username { get; set; } = string.Empty;

        [FromForm(Name = "password")]
        public string Password { get; set; } = string.Empty;
    }
}
