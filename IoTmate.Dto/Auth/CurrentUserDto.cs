namespace IoTmate.Dto.Auth;

public record CurrentUserDto
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
}