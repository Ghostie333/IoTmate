using IoTmate.Dto.Device;
using IoTmate.Services.Devices;
using Microsoft.AspNetCore.Mvc;

namespace IoTmate.Api.Controllers
{
    [ApiController]
    [Route("api/mqtt/auth")]
    public class MqttAuthController(IDeviceService deviceService) : ControllerBase
    {
        private readonly IDeviceService _deviceService = deviceService;

        [HttpPost("user")]
        public async Task<IActionResult> AuthenticateUser([FromForm] MqttUserAuthRequestDto request)
        {
            bool isValid = await _deviceService.ValidateCredentialsAsync(request.Username, request.Password);
            return isValid ? Ok() : Unauthorized();
        }

        // Called by mosquitto-go-auth to check ACL permissions when a device attempts to publish or subscribe to a topic.
        [HttpPost("acl")]
        public IActionResult CheckAcl([FromForm] MqttAclCheckRequestDto request)
        {
            bool isAllowed = _deviceService.IsTopicAllowedForDevice(request.Username, request.Topic);
            return isAllowed ? Ok() : Unauthorized();
        }
    }
}