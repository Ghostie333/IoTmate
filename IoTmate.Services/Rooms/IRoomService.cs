using IoTmate.Dto.Room;
using IoTmate.Dto.Device;
using IoTmate.Model.Enums;
using IoTmate.Model.Entities;

namespace IoTmate.Services.Rooms
{
    public interface IRoomService
    {
        // User
        Task<List<RoomDto>> GetUserRooms(string currentUserId);
        Task<RoomDto> GetRoomById(Guid roomId, string currentUserId);

        // Room
        Task<RoomDto> CreateRoom(CreateRoomInputDto input, string currentUserId);
        Task UpdateRoom(CreateRoomInputDto input, Guid roomId, string currentUserId);
        Task<bool> DeleteRoom(Guid roomId, string currentUserId);

        // Room users
        Task<bool> AddUserToRoom(Guid roomId, string currentUserId, string userId, RoomRole roomRole);
        Task<bool> DeleteUserFromRoom(Guid roomId, string currentUserId, string userId);
        Task<RoomRole?> GetUserRole(Guid roomId, string currentUserId);
        Task<List<RoomMemberDto>> GetRoomMembers(Guid roomId, string currentUserId);

        // Devices
        Task<List<DeviceDto>> GetRoomDevices(Guid roomId, string currentUserId);
        Task<bool> AddDeviceToRoom(Guid roomId, string currentUserId, string deviceId);
        Task<bool> DeleteDeviceFromRoom(Guid roomId, string currentUserId, string deviceId);
    }
}
