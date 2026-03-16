using IoTmate.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthDebugController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public AuthDebugController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            authenticated = User.Identity?.IsAuthenticated == true,
            userId = _currentUser.UserId,
            name = User.Identity?.Name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray()
        });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new { ok = true, userId = _currentUser.UserId });
    }
}
