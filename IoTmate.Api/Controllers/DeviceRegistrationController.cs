using IoTmate.Dto.Device;
using IoTmate.Services.Devices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IoTmate.Api.Controllers;

[ApiController]
[Route("api/devices")]
public class DeviceRegistrationController(IDeviceService deviceService) : ControllerBase
{
    private readonly IDeviceService _deviceService = deviceService;
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("register")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequestDto request)
    {
        var result = await _deviceService.RegisterDeviceAsync(request, HttpContext.RequestAborted);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid device secret for the provided DeviceId (MAC)." });
        }

        return Ok(new RegisterDeviceResponseDto
        {
            DeviceId = result.Device.DeviceId,
            SecretToken = result.RawSecretToken
        });
    }

    [Authorize]
    [HttpPatch("{deviceId}")]
    public async Task<IActionResult> UpdateDevice([FromRoute] string deviceId, [FromBody] UpdateDeviceRequestDto request)
    {
        await _deviceService.UpdateDeviceAsync(deviceId, request.NewName, CurrentUserId, HttpContext.RequestAborted);
        return Ok();
    }
}