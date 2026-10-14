using IoTmate.Model.Entities;
using IoTmate.Model.Enums;

namespace IoTmate.Repository.Rooms
{
    public interface IRoomRepository
    {
        // User rooms
        Task<Room?> GetRoomByIdAsync(Guid roomId, CancellationToken ct);
        Task<List<Room>> GetUserRoomsAsync(string userId, CancellationToken ct);

        // Create, update, delete room
        Task<Room> CreateRoomWithOwnerAsync(Room room, string ownerUserId, CancellationToken ct);
        Task UpdateRoomAsync(Room room, CancellationToken ct);
        Task<bool> DeleteRoomAsync(Guid roomId, CancellationToken ct);

        // Room users
        Task<bool> AddUserToRoomAsync(Guid roomId, string userId, RoomRole roomRole, CancellationToken ct);
        Task<bool> DeleteUserFromRoomAsync(Guid roomId, string userId, CancellationToken ct);
        Task<RoomRole?> GetUserRoleAsync(Guid roomId, string userId, CancellationToken ct);
        Task<List<RoomMember>> GetRoomMembersAsync(Guid roomId, CancellationToken ct);

        // Devices
        Task<List<Device>> GetRoomDevicesAsync(Guid roomId, CancellationToken ct);
        Task<bool> AddDeviceToRoomAsync(Guid roomId, string deviceId, CancellationToken ct);
        Task<bool> DeleteDeviceFromRoomAsync(Guid roomId, string deviceId, CancellationToken ct);
    }
}
