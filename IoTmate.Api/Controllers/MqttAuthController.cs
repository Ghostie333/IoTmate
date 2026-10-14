using IoTmate.Dto.Device;
using IoTmate.Services.Devices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace IoTmate.Api.Controllers
{
    [ApiController]
    [Route("api/mqtt/auth")]
    public class MqttAuthController(IDeviceService deviceService, IConfiguration configuration) : ControllerBase
    {
        private readonly IDeviceService _deviceService = deviceService;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("user")]
        public async Task<IActionResult> AuthenticateUser([FromForm] MqttUserAuthRequestDto request)
        {
            // Check if user is IoTmate backend, Timing attack protection
            if (IsBackendServiceUsername(request.Username) && IsBackendServicePassword(request.Password))
            {
                // Inform go-auth extension that user is superuser (Admin)
                Response.Headers.Append("X-Superuser", "true");
                return Ok();

            }

            // Check for not-backend devices
            bool isValid = await _deviceService.ValidateCredentialsAsync(request.Username, request.Password, HttpContext.RequestAborted);
            return isValid ? Ok() : Unauthorized();
        }

        // Called by mosquitto-go-auth to check ACL permissions when a device attempts to publish or subscribe to a topic
        [HttpPost("acl")]
        public IActionResult CheckAcl([FromForm] MqttAclCheckRequestDto request)
        {
            if (IsBackendServiceUsername(request.Username))
                return Ok();

            bool isAllowed = _deviceService.IsTopicAllowedForDevice(request.Username, request.Topic);
            return isAllowed ? Ok() : Unauthorized();
        }

        #region Private helpers
        // Helper for checking if username is IoTmate-backend user username
        private bool IsBackendServiceUsername(string username)
        {
            var expectedUsername = _configuration["MqttSettings:Username"];
            if (string.IsNullOrEmpty(expectedUsername) || string.IsNullOrEmpty(username))
                return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(username),
                Encoding.UTF8.GetBytes(expectedUsername)
            );
        }


        // Helper for checking if password is IoTmate-backend user password
        private bool IsBackendServicePassword(string password)
        {
            var expectedPassword = _configuration["MqttSettings:Password"];
            if (string.IsNullOrEmpty(expectedPassword) || string.IsNullOrEmpty(password))
                return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(expectedPassword)
            );
        }

        #endregion
    }
}