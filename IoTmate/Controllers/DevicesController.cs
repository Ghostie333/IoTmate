using IoTmate.Contracts.Devices;
using IoTmate.Authentication;
using IoTmate.Infrastructure.Mqtt;
using IoTmate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize]
public sealed class DevicesController : ControllerBase
{
    private readonly IDeviceStore _devices;
    private readonly IMqttPublisher _mqtt;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;

    public DevicesController(IDeviceStore devices, IMqttPublisher mqtt, IConfiguration configuration, ICurrentUser currentUser)
    {
        _devices = devices;
        _mqtt = mqtt;
        _configuration = configuration;
        _currentUser = currentUser;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<DeviceDto>> GetAll()
        => Ok(_devices.GetAll(RequireUserId()));

    [HttpGet("{id}")]
    public ActionResult<DeviceDto> GetById(string id)
    {
        var device = _devices.GetById(RequireUserId(), id);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpPost("{id}/cmd/{command}")]
    public async Task<IActionResult> SendCommand(string id, string command, [FromBody] CommandRequestDto request, CancellationToken cancellationToken)
    {
        if (_devices.GetById(RequireUserId(), id) is null)
        {
            return NotFound();
        }

        var prefix = _configuration["Mqtt:CommandTopicPrefix"] ?? "devices/";
        var topic = $"{prefix.TrimEnd('/')}/{id}/cmd/{command}";

        await _mqtt.PublishAsync(topic, request.PayloadJson, cancellationToken);
        return Accepted(new { topic });
    }

    private string RequireUserId()
        => _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated");
}
