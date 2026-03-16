using IoTmate.Authentication;
using IoTmate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Controllers;

[ApiController]
[Route("api/dev/admin")]
[Authorize]
public sealed class DevAdminController : ControllerBase
{
    private readonly IDeviceStore _devices;
    private readonly ICurrentUser _currentUser;

    public DevAdminController(IDeviceStore devices, ICurrentUser currentUser)
    {
        _devices = devices;
        _currentUser = currentUser;
    }

    [HttpGet("devices")]
    public IActionResult MyDevices()
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        return Ok(_devices.GetAll(userId));
    }

    [HttpDelete("devices/{deviceId}")]
    public IActionResult DeleteDevice(string deviceId)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        // Best-effort: implementacja zależy od store.
        if (_devices is MongoDeviceStore mongo)
        {
            mongo.Delete(userId, deviceId);
            return NoContent();
        }

        return StatusCode(501, "Delete not supported by this store");
    }
}
