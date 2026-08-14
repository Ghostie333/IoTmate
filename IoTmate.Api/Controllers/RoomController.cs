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

        [HttpGet]
        public async Task<IActionResult> GetUserRooms()
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roomDto = await _roomService.GetUserRooms(currentUser!);
            return Ok(roomDto);
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoomById([FromRoute] Guid roomId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roomDto = await _roomService.GetRoomById(roomId, currentUser!);
            return Ok(roomDto);
        }

        [HttpPost("new")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomInputDto input)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var newRoomDto = await _roomService.CreateRoom(input, currentUser!);
            return Ok(newRoomDto);
        }

        [HttpPut("{roomId}")]
        public async Task<IActionResult> UpdateRoom([FromBody] CreateRoomInputDto input, [FromRoute] Guid roomId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _roomService.UpdateRoom(input, roomId, currentUser!);
            return Ok();
        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> DeleteRoom([FromRoute] Guid roomId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isSuccess = await _roomService.DeleteRoom(roomId, currentUser!);
            if (!isSuccess) return NotFound();
            return Ok();
        }

        [HttpGet("{roomId}/members")]
        public async Task<IActionResult> GetRoomMembers([FromRoute] Guid roomId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roomMembers = await _roomService.GetRoomMembers(roomId, currentUser!);
            return Ok(roomMembers);

        }

        [HttpGet("{roomId}/devices")]
        public async Task<IActionResult> GetRoomDevices([FromRoute] Guid roomId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roomDevices = await _roomService.GetRoomDevices(roomId, currentUser!);
            return Ok(roomDevices);
        }

        [HttpPost("{roomId}/devices")]
        public async Task<IActionResult> AddDeviceToRoom([FromRoute] Guid roomId, [FromBody] string deviceId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _roomService.AddDeviceToRoom(roomId, currentUser!, deviceId);
            return Ok();
        }

        [HttpDelete("{roomId}/devices")]
        public async Task<IActionResult> DeleteDeviceFromRoom([FromRoute] Guid roomId, [FromBody] string deviceId)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isSuccess = await _roomService.DeleteDeviceFromRoom(roomId, currentUser!, deviceId);
            if (!isSuccess) return BadRequest();
            return Ok();
        }
    }
}
