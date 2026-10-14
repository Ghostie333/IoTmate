using IoTmate.Dto.Room;
using IoTmate.Services.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IoTmate.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/rooms")]
    public class RoomController(IRoomService roomService) : ControllerBase
    {
        private readonly IRoomService _roomService = roomService;
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        [HttpGet]
        public async Task<IActionResult> GetUserRooms()
        {
            var roomDto = await _roomService.GetUserRooms(CurrentUserId, HttpContext.RequestAborted);
            return Ok(roomDto);
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoomById([FromRoute] Guid roomId)
        {
            var roomDto = await _roomService.GetRoomById(roomId, CurrentUserId, HttpContext.RequestAborted);
            return Ok(roomDto);
        }

        [HttpPost("new")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomInputDto input)
        {
            var newRoomDto = await _roomService.CreateRoom(input, CurrentUserId, HttpContext.RequestAborted);
            return Ok(newRoomDto);
        }

        [HttpPut("{roomId}")]
        public async Task<IActionResult> UpdateRoom([FromBody] CreateRoomInputDto input, [FromRoute] Guid roomId)
        {
            await _roomService.UpdateRoom(input, roomId, CurrentUserId, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> DeleteRoom([FromRoute] Guid roomId)
        {
            bool isSuccess = await _roomService.DeleteRoom(roomId, CurrentUserId, HttpContext.RequestAborted);
            if (!isSuccess) return NotFound();
            return Ok();
        }

        [HttpGet("{roomId}/members")]
        public async Task<IActionResult> GetRoomMembers([FromRoute] Guid roomId)
        {
            var roomMembers = await _roomService.GetRoomMembers(roomId, CurrentUserId, HttpContext.RequestAborted);
            return Ok(roomMembers);

        }

        [HttpGet("{roomId}/devices")]
        public async Task<IActionResult> GetRoomDevices([FromRoute] Guid roomId)
        {
            var roomDevices = await _roomService.GetRoomDevices(roomId, CurrentUserId, HttpContext.RequestAborted);
            return Ok(roomDevices);
        }

        [HttpPost("{roomId}/devices")]
        public async Task<IActionResult> AddDeviceToRoom([FromRoute] Guid roomId, [FromBody] string deviceId)
        {
            await _roomService.AddDeviceToRoom(roomId, CurrentUserId, deviceId, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpDelete("{roomId}/devices")]
        public async Task<IActionResult> DeleteDeviceFromRoom([FromRoute] Guid roomId, [FromBody] string deviceId)
        {
            bool isSuccess = await _roomService.DeleteDeviceFromRoom(roomId, CurrentUserId, deviceId, HttpContext.RequestAborted);
            if (!isSuccess) return BadRequest();
            return Ok();
        }
    }
}
