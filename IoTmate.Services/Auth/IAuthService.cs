using IoTmate.Dto.Auth;

namespace IoTmate.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterUserAsync(UserRegistrationInputDto input);
        Task<AuthResultDto> LoginUserAsync(UserLoginInputDto input);
        Task LogoutUserAsync();
    }
}
