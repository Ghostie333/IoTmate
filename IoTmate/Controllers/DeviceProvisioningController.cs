using IoTmate.Authentication;
using IoTmate.Contracts.Devices;
using IoTmate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Controllers;

[ApiController]
[Route("api/device-provisioning")]
[Authorize]
public sealed class DeviceProvisioningController : ControllerBase
{
    private readonly IDeviceStore _devices;
    private readonly ICurrentUser _currentUser;

    public DeviceProvisioningController(IDeviceStore devices, ICurrentUser currentUser)
    {
        _devices = devices;
        _currentUser = currentUser;
    }

    public sealed record AddDeviceRequest(string DeviceId, string Name);

    public sealed class AddDeviceForm
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    [HttpPost("add")]
    public ActionResult<DeviceDto> Add([FromBody] AddDeviceRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            return BadRequest("DeviceId is required");
        }

        var deviceId = request.DeviceId.Trim();

        try
        {
            var device = new DeviceDto(deviceId, string.IsNullOrWhiteSpace(request.Name) ? deviceId : request.Name.Trim(), Online: false, LastSeenUtc: null);
            _devices.Upsert(userId, device);
            return Ok(device);
        }
        catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError?.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey)
        {
            return Conflict("DeviceId is already taken.");
        }
        catch (MongoDB.Driver.MongoBulkWriteException<IoTmate.Data.Devices.DeviceDocument> ex) when (ex.WriteErrors.Any(e => e.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey))
        {
            return Conflict("DeviceId is already taken.");
        }
    }

    [HttpPost("add-form")]
    [IgnoreAntiforgeryToken]
    public IActionResult AddForm([FromForm] AddDeviceForm request)
    {
        var result = Add(new AddDeviceRequest(request.DeviceId, request.Name));
        if (result.Result is ObjectResult { StatusCode: 409 } || result.Result is ConflictObjectResult)
        {
            return Redirect("/devices/add?error=DeviceId%20is%20already%20taken");
        }

        if (result.Result is BadRequestObjectResult)
        {
            return Redirect("/devices/add?error=Invalid%20device%20data");
        }

        if (result.Result is UnauthorizedResult)
        {
            return Redirect("/login");
        }

        return Redirect("/devices");
    }
}
