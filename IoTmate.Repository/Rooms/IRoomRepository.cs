using IoTmate.Model.Entities;
using IoTmate.Model.Enums;

namespace IoTmate.Repository.Rooms
{
    public interface IRoomRepository
    {
        // User rooms
        Task<Room?> GetRoomByIdAsync(Guid roomId);
        Task<List<Room>> GetUserRoomsAsync(string userId);

        // Create, update, delete room
        Task<Room> CreateRoomAsync(Room room);
        Task<Room> CreateRoomWithOwnerAsync(Room room, string ownerUserId);
        Task UpdateRoomAsync(Room room);
        Task<bool> DeleteRoomAsync(Guid roomId);

        // Room users
        Task<bool> AddUserToRoomAsync(Guid roomId, string userId, RoomRole roomRole);
        Task<bool> DeleteUserFromRoomAsync(Guid roomId, string userId);
        Task<RoomRole?> GetUserRoleAsync(Guid roomId, string userId);
        Task<List<RoomMember>> GetRoomMembersAsync(Guid roomId);

        // Devices
        Task<List<Device>> GetRoomDevicesAsync(Guid roomId);
        Task<bool> AddDeviceToRoomAsync(Guid roomId, string deviceId);
        Task<bool> DeleteDeviceFromRoomAsync(Guid roomId, string deviceId);
    }
}
