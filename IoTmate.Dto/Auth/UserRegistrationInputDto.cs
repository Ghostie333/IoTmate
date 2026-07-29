using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTmate.Dto.Auth
{
    public record UserRegistrationInputDto
    {
        [Required(ErrorMessage = "Username is required")]
        public required string UserName { get; init; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Wrong email adress")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; init; }

        [Compare("Password", ErrorMessage = "Passwords must be identical")]
        public required string ConfirmPassword { get; init; }
    }
}
