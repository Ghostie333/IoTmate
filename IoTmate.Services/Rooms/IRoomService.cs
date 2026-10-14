using IoTmate.Dto.Room;
using IoTmate.Dto.Device;
using IoTmate.Model.Enums;
using IoTmate.Model.Entities;

namespace IoTmate.Services.Rooms
{
    public interface IRoomService
    {
        // User
        Task<List<RoomDto>> GetUserRooms(string currentUserId, CancellationToken ct);
        Task<RoomDto> GetRoomById(Guid roomId, string currentUserId, CancellationToken ct);

        // Room
        Task<RoomDto> CreateRoom(CreateRoomInputDto input, string currentUserId, CancellationToken ct);
        Task UpdateRoom(CreateRoomInputDto input, Guid roomId, string currentUserId, CancellationToken ct);
        Task<bool> DeleteRoom(Guid roomId, string currentUserId, CancellationToken ct);

        // Room users
        Task<bool> AddUserToRoom(Guid roomId, string currentUserId, string userId, RoomRole roomRole, CancellationToken ct);
        Task<bool> DeleteUserFromRoom(Guid roomId, string currentUserId, string userId, CancellationToken ct);
        Task<RoomRole?> GetUserRole(Guid roomId, string currentUserId, CancellationToken ct);
        Task<List<RoomMemberDto>> GetRoomMembers(Guid roomId, string currentUserId, CancellationToken ct);

        // Devices
        Task<List<DeviceDto>> GetRoomDevices(Guid roomId, string currentUserId, CancellationToken ct);
        Task AddDeviceToRoom(Guid roomId, string currentUserId, string deviceId, CancellationToken ct);
        Task<bool> DeleteDeviceFromRoom(Guid roomId, string currentUserId, string deviceId, CancellationToken ct);
    }
}
