using IoTmate.Common.Exceptions;
using IoTmate.Services.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IoTmate.Services.Hubs
{
    [Authorize]
    public class DeviceHub(IRoomService roomService) : Hub<IDeviceHubClient>
    {
        private readonly IRoomService _roomService = roomService;
        public async Task JoinRoom(Guid roomId, CancellationToken ct)
        {
            try
            {
                await _roomService.GetUserRole(roomId, Context.UserIdentifier!, ct);
            }
            catch (ForbiddenException)
            {
                throw new HubException("hubs.notAuthorized");
            }
            catch (ArgumentException)
            {
                throw new HubException("hubs.invalidRoom");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }

        public async Task LeaveRoom(Guid roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }
    }
}
