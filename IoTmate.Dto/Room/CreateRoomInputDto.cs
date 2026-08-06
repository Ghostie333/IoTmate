using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Room
{
    public record CreateRoomInputDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Room name is required")]
        public required string Name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Time zone is required")]
        public required string TimeZoneId { get; set; }
    }
}
