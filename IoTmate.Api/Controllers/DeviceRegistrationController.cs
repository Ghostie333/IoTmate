using IoTmate.Dto.Device;
using IoTmate.Services.Devices;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Api.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DeviceRegistrationController(IDeviceService deviceService) : ControllerBase
    {
        private readonly IDeviceService _deviceService = deviceService;

        [HttpPost("register")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequestDto request)
        {
            // Register device with service
            var rawSecretToken = await _deviceService.RegisterDeviceAsync(request.DeviceId, request.DeviceSecret);

            if (rawSecretToken == null)
            {
                return Conflict(new { message = "Failed to get secretToken" });
            }

            var normalizedMac = request.DeviceId.Trim().ToUpperInvariant();

            // Return status 200 and token
            return Ok(new RegisterDeviceResponseDto
            {
                DeviceId = normalizedMac,
                SecretToken = rawSecretToken
            });
        }
    }
}