using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Device
{
    public record MqttAclCheckRequestDto
    {
        [FromForm(Name = "username")]
        public string Username { get; set; } = string.Empty;

        [FromForm(Name = "topic")]
        public string Topic { get; set; } = string.Empty;

        [FromForm(Name = "acc")]
        public int Acc { get; set; } // 1 = Read, 2 = Write
    }
}
