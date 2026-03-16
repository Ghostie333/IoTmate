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
    private readonly ILogger<DeviceProvisioningController> _logger;

    public DeviceProvisioningController(IDeviceStore devices, ICurrentUser currentUser, ILogger<DeviceProvisioningController> logger)
    {
        _devices = devices;
        _currentUser = currentUser;
        _logger = logger;
    }

    public sealed record AddDeviceRequest(string DeviceId, string Name);

    public sealed class AddDeviceForm
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    [HttpPost("add")]
    public IActionResult Add([FromBody] AddDeviceRequest request)
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
        _logger.LogInformation("Provisioning device. UserId={UserId} DeviceId={DeviceId} Name={Name}", userId, deviceId, request.Name);

        try
        {
            var device = new DeviceDto(deviceId, string.IsNullOrWhiteSpace(request.Name) ? deviceId : request.Name.Trim(), Online: false, LastSeenUtc: null);
            _devices.Upsert(userId, device);
            return Ok(device);
        }
        catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError?.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(ex, "DuplicateKey while provisioning. UserId={UserId} DeviceId={DeviceId} Code={Code} Message={Message}", userId, deviceId, ex.WriteError?.Code, ex.WriteError?.Message);
            return Conflict($"DeviceId '{deviceId}' is already taken.");
        }
        catch (MongoDB.Driver.MongoBulkWriteException<IoTmate.Data.Devices.DeviceDocument> ex) when (ex.WriteErrors.Any(e => e.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey))
        {
            _logger.LogWarning(ex, "DuplicateKey (bulk) while provisioning. UserId={UserId} DeviceId={DeviceId} Errors={Errors}", userId, deviceId, ex.WriteErrors.Select(e => new { e.Code, e.Message }).ToArray());
            return Conflict($"DeviceId '{deviceId}' is already taken.");
        }
    }

    [HttpPost("add-form")]
    public IActionResult AddForm([FromForm] AddDeviceForm request)
    {
        var result = Add(new AddDeviceRequest(request.DeviceId, request.Name));
        if (result is UnauthorizedResult)
        {
            return Redirect("/login?returnUrl=/devices/add");
        }

        if (result is ObjectResult { StatusCode: 409 } || result is ConflictObjectResult)
        {
            return Redirect("/devices/add?error=DeviceId%20is%20already%20taken");
        }

        if (result is BadRequestObjectResult)
        {
            return Redirect("/devices/add?error=Invalid%20device%20data");
        }

        return Redirect("/devices");
    }
}
