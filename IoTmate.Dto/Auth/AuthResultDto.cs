using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Dto.Auth
{
    public record AuthResultDto
    {
        public bool Succeeded { get; init; }

        // Error list
        public IEnumerable<string> Errors { get; init; } = [];
    }
}
