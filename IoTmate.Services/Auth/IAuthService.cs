using IoTmate.Dto.Auth;

namespace IoTmate.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterUserAsync(UserRegistrationInputDto input, CancellationToken ct);
        Task<AuthResultDto> LoginUserAsync(UserLoginInputDto input, CancellationToken ct);
        Task LogoutUserAsync(CancellationToken ct);
        Task<CurrentUserDto?> GetCurrentUserAsync(string? userId, CancellationToken ct);
    }
}
