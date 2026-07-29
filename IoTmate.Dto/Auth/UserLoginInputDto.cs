using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Auth
{
    public record UserLoginInputDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Wrong email address")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; init; }

        public bool IsPersistent { get; init; } = false;
    }
}
