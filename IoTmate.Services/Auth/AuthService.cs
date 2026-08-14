using IoTmate.Model.Entities;
using IoTmate.Dto.Auth;
using Microsoft.AspNetCore.Identity;

namespace IoTmate.Services.Auth
{
    public class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        public async Task<AuthResultDto> RegisterUserAsync(UserRegistrationInputDto input)
        {
            // Check if user with this email doesnt exists
            var existingUser = await _userManager.FindByEmailAsync(input.Email);
            if (existingUser != null)
                return new AuthResultDto { Succeeded = false, Errors = ["Account with this email already exists"] };


            var user = new ApplicationUser
            {
                UserName = input.UserName,
                Email = input.Email
            };

            // Save new user in database
            var result = await _userManager.CreateAsync(user, input.Password);

            if (!result.Succeeded)
            {
                // Get error list from IdentityResult
                var errorMessages = result.Errors.Select(e => e.Description);

                return new AuthResultDto
                {
                    Succeeded = false,
                    Errors = errorMessages
                };

            }

            // Auto login user
            await SignInUserAsync(user, true);

            return new AuthResultDto { Succeeded = true };
        }

        public async Task<AuthResultDto> LoginUserAsync(UserLoginInputDto input)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);

            if(user == null || !await _userManager.CheckPasswordAsync(user, input.Password))
                return new AuthResultDto { Succeeded = false, Errors = ["Wrong email or password"] };

            await SignInUserAsync(user, input.IsPersistent);

            return new AuthResultDto { Succeeded = true };
        }

        private async Task SignInUserAsync(ApplicationUser user, bool isPersistent)
        {
            // isPersistent = true - cookie remains after closing the browser
            await _signInManager.SignInAsync(user, isPersistent);
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<CurrentUserDto?> GetCurrentUserAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new CurrentUserDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? ""
            };
        }

    }
}
