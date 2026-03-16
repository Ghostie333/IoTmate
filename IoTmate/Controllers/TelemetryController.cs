using IoTmate.Contracts.Telemetry;
using IoTmate.Authentication;
using IoTmate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Controllers;

[ApiController]
[Route("api/telemetry")]
[Authorize]
public sealed class TelemetryController : ControllerBase
{
    private readonly ITelemetryBus _telemetry;
    private readonly IDeviceStore _devices;
    private readonly ICurrentUser _currentUser;

    public TelemetryController(ITelemetryBus telemetry, IDeviceStore devices, ICurrentUser currentUser)
    {
        _telemetry = telemetry;
        _devices = devices;
        _currentUser = currentUser;
    }

    [HttpGet("{deviceId}")]
    public ActionResult<IReadOnlyList<TelemetryPointDto>> GetLatest(
        string deviceId,
        [FromQuery] int take = 50)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        if (_devices.GetById(userId, deviceId) is null)
        {
            return NotFound();
        }

        return Ok(_telemetry.GetLatest(deviceId, take));
    }
}
