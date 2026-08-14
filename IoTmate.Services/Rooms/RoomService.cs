using AutoMapper;
using IoTmate.Common.Exceptions;
using IoTmate.Dto.Device;
using IoTmate.Dto.Room;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using IoTmate.Repository.Devices;
using IoTmate.Repository.Rooms;

namespace IoTmate.Services.Rooms
{
    public class RoomService(IRoomRepository roomRepository, IDeviceRepository deviceRepository, IMapper mapper) : IRoomService
    {
        private readonly IRoomRepository _roomRepository = roomRepository;
        private readonly IDeviceRepository _deviceRepository = deviceRepository;
        private readonly IMapper _mapper = mapper;

        // User
        public async Task<List<RoomDto>> GetUserRooms(string currentUserId)
        {
            EnsureAuthenticated(currentUserId);

            var userRooms = await _roomRepository.GetUserRoomsAsync(currentUserId);

            return _mapper.Map<List<RoomDto>>(userRooms);
        }

        public async Task<RoomDto> GetRoomById(Guid roomId, string currentUserId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);

            await EnsureRoomAccessAsync(roomId, currentUserId);

            var room = await _roomRepository.GetRoomByIdAsync(roomId)
                ?? throw new NotFoundException($"Room with id '{roomId}' was not found.");

            return _mapper.Map<RoomDto>(room);
        }

        // Room
        public async Task<RoomDto> CreateRoom(CreateRoomInputDto input, string currentUserId)
        {
            ArgumentNullException.ThrowIfNull(input);
            EnsureAuthenticated(currentUserId);

            var room = _mapper.Map<Room>(input);
            var createdRoom = await _roomRepository.CreateRoomWithOwnerAsync(room, currentUserId);

            return _mapper.Map<RoomDto>(createdRoom);
        }

        public async Task UpdateRoom(CreateRoomInputDto input, Guid roomId, string currentUserId)
        {
            ArgumentNullException.ThrowIfNull(input);
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);

            await EnsureRoomAccessAsync(roomId, currentUserId);

            var room = await _roomRepository.GetRoomByIdAsync(roomId)
                ?? throw new NotFoundException($"Room with id '{roomId}' was not found.");

            await EnsureRoomOwnerAsync(roomId, currentUserId);

            _mapper.Map(input, room);
            await _roomRepository.UpdateRoomAsync(room);
        }

        public async Task<bool> DeleteRoom(Guid roomId, string currentUserId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);

            await EnsureRoomAccessAsync(roomId, currentUserId);

            _ = await _roomRepository.GetRoomByIdAsync(roomId)
                ?? throw new NotFoundException($"Room with id '{roomId}' was not found.");

            await EnsureRoomOwnerAsync(roomId, currentUserId);

            return await _roomRepository.DeleteRoomAsync(roomId);
        }

        // Room users
        public async Task<bool> AddUserToRoom(Guid roomId, string currentUserId, string userId, RoomRole roomRole)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);
            EnsureAuthenticated(userId);

            await EnsureRoomAccessAsync(roomId, currentUserId);
            await EnsureRoomOwnerAsync(roomId, currentUserId);

            return await _roomRepository.AddUserToRoomAsync(roomId, userId, roomRole);
        }

        public async Task<bool> DeleteUserFromRoom(Guid roomId, string currentUserId, string userId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);
            EnsureAuthenticated(userId);

            await EnsureRoomAccessAsync(roomId, currentUserId);
            await EnsureRoomOwnerAsync(roomId, currentUserId);

            return await _roomRepository.DeleteUserFromRoomAsync(roomId, userId);
        }

        public async Task<RoomRole?> GetUserRole(Guid roomId, string currentUserId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);

            await EnsureRoomAccessAsync(roomId, currentUserId);

            return await _roomRepository.GetUserRoleAsync(roomId, currentUserId);
        }

        public async Task<List<RoomMemberDto>> GetRoomMembers(Guid roomId, string currentUserId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);

            await EnsureRoomAccessAsync(roomId, currentUserId);

            var members = await _roomRepository.GetRoomMembersAsync(roomId);

            return [.. members
                .Select(member => new RoomMemberDto
                {
                    UserId = member.UserId,
                    Username = member.User?.UserName ?? "Unknown",
                    Role = member.Role
                })];
        }

        // Devices
        public async Task<List<DeviceDto>> GetRoomDevices(Guid roomId, string currentUserId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);

            await EnsureRoomAccessAsync(roomId, currentUserId);

            var roomDevices = await _roomRepository.GetRoomDevicesAsync(roomId);

            return _mapper.Map<List<DeviceDto>>(roomDevices);
        }
        public async Task AddDeviceToRoom(Guid roomId, string currentUserId, string deviceId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);
            EnsureValidDeviceId(deviceId);

            await EnsureRoomAccessAsync(roomId, currentUserId);
            await EnsureRoomOwnerAsync(roomId, currentUserId);

            var device = await _deviceRepository.GetByDeviceIdAsync(deviceId);
            if (device == null)
                throw new NotFoundException("device.notFound");

            if (device.RoomId != null)
                throw new ConflictException("device.alreadyAssigned");

            if (!await _roomRepository.AddDeviceToRoomAsync(roomId, deviceId))
                throw new ConflictException("device.alreadyAssigned");
        }
        public async Task<bool> DeleteDeviceFromRoom(Guid roomId, string currentUserId, string deviceId)
        {
            EnsureValidRoomId(roomId);
            EnsureAuthenticated(currentUserId);
            EnsureValidDeviceId(deviceId);

            await EnsureRoomAccessAsync(roomId, currentUserId);
            await EnsureRoomOwnerAsync(roomId, currentUserId);

            return await _roomRepository.DeleteDeviceFromRoomAsync(roomId, deviceId);
        }

        // Guards
        private async Task EnsureRoomAccessAsync(Guid roomId, string currentUserId)
        {
            _ = await _roomRepository.GetUserRoleAsync(roomId, currentUserId)
                ?? throw new ForbiddenException("You don't have access to this room.");
        }

        private async Task EnsureRoomOwnerAsync(Guid roomId, string currentUserId)
        {
            var role = await _roomRepository.GetUserRoleAsync(roomId, currentUserId);
            if (role != RoomRole.Owner)
                throw new ForbiddenException("Only the room owner can perform this action.");
        }
        private static void EnsureAuthenticated(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));
        }
        private static void EnsureValidRoomId(Guid roomId)
        {
            if (roomId == Guid.Empty)
                throw new ArgumentException("Room id cannot be empty.", nameof(roomId));
        }
        private static void EnsureValidDeviceId(string deviceId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(deviceId, nameof(deviceId));
        }
    }
}
