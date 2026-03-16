using System.ComponentModel.DataAnnotations;
using IoTmate.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IoTmate.Pages;

public sealed class RegisterModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterModel(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? Error { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!string.Equals(Input.Password, Input.ConfirmPassword, StringComparison.Ordinal))
        {
            Error = "Passwords do not match.";
            return Page();
        }

        var user = new AppUser
        {
            UserName = Input.Email,
            Email = Input.Email
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            return LocalRedirect("/login");
        }

        Error = string.Join("\n", result.Errors.Select(e => e.Description));
        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
