using IoTmate.Model;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace IoTmate.Repository.Rooms
{
    public class RoomRepository(ApplicationDbContext dbContext) : BaseRepository(dbContext), IRoomRepository
    {
        // User rooms
        public async Task<Room?> GetRoomByIdAsync(Guid roomId, CancellationToken ct)
        {
            if (roomId == Guid.Empty)
                return null;

            return await DbContext.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, ct);
        }
        public async Task<List<Room>> GetUserRoomsAsync(string userId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(userId))
                return [];

            var rooms = await DbContext.Rooms.Where(r => r.Members.Any(u => u.UserId == userId)).ToListAsync(ct);

            return rooms;
        }

        // Create, update, delete room
        public async Task<Room> CreateRoomWithOwnerAsync(Room room, string ownerUserId, CancellationToken ct)
        {
            room.Members.Add(new RoomMember
            {
                RoomId = room.Id,
                UserId = ownerUserId,
                Role = RoomRole.Owner,
                JoinedAt = DateTime.UtcNow
            });

            DbContext.Rooms.Add(room);
            await DbContext.SaveChangesAsync(ct);

            return room;
        }
        public async Task UpdateRoomAsync(Room room, CancellationToken ct)
        {
            if (DbContext.Entry(room).State == EntityState.Detached)
                DbContext.Rooms.Update(room);

            await DbContext.SaveChangesAsync(ct);
        }
        public async Task<bool> DeleteRoomAsync(Guid roomId, CancellationToken ct)
        {
            if (roomId == Guid.Empty)
                return false;

            var room = await DbContext.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, ct);
            if (room == null)
                return false;

            DbContext.Rooms.Remove(room);
            await DbContext.SaveChangesAsync(ct);

            return true;
        }

        // Room users
        public async Task<bool> AddUserToRoomAsync(Guid roomId, string userId, RoomRole roomRole, CancellationToken ct)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(userId))
                return false;

            var userExists = await DbContext.Users.AnyAsync(u => u.Id == userId, ct);
            var roomExists = await DbContext.Rooms.AnyAsync(r => r.Id == roomId, ct);

            if (!userExists || !roomExists)
                return false;

            if (await DbContext.RoomMembers.AnyAsync(m => m.RoomId == roomId && m.UserId == userId, ct))
                return false;

            RoomMember newRoomMember = new()
            {
                RoomId = roomId,
                UserId = userId,
                Role = roomRole,
                JoinedAt = DateTime.UtcNow
            };

            await DbContext.RoomMembers.AddAsync(newRoomMember, ct);
            await DbContext.SaveChangesAsync(ct);

            return true;
        }
        public async Task<bool> DeleteUserFromRoomAsync(Guid roomId, string userId, CancellationToken ct)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(userId))
                return false;

            var room = await DbContext.Rooms.Include(r => r.Members).FirstOrDefaultAsync(r => r.Id == roomId, ct);
            if (room == null)
                return false;

            var member = room.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
                return false;

            room.Members.Remove(member);
            await DbContext.SaveChangesAsync(ct);

            return true;
        }
        public async Task<RoomRole?> GetUserRoleAsync(Guid roomId, string userId, CancellationToken ct)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(userId))
                return null;

            return await DbContext.RoomMembers
                .AsNoTracking()
                .Where(m => m.RoomId == roomId && m.UserId == userId)
                .Select(m => (RoomRole?)m.Role)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<List<RoomMember>> GetRoomMembersAsync(Guid roomId, CancellationToken ct)
        {
            if (roomId == Guid.Empty)
                return [];

            var members = await DbContext.RoomMembers
                .Where(m => m.RoomId == roomId)
                .Include(m => m.User)
                .ToListAsync(ct);

            return members;
        }

        // Devices
        public async Task<List<Device>> GetRoomDevicesAsync(Guid roomId, CancellationToken ct)
        {
            if (roomId == Guid.Empty)
                return [];

            var room = await DbContext.Rooms.Include(r => r.Devices).FirstOrDefaultAsync(r => r.Id == roomId, ct);
            if (room == null)
                return [];

            var roomDevices = room.Devices.ToList();

            return roomDevices;
        }
        public async Task<bool> AddDeviceToRoomAsync(Guid roomId, string deviceId, CancellationToken ct)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(deviceId))
                return false;

            var normalizedDeviceId = deviceId.Trim().ToUpperInvariant();

            var updated = await DbContext.Devices
                .Where(d => d.DeviceId == normalizedDeviceId && d.RoomId == null)
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.RoomId, roomId), ct);

            return updated > 0;
        }
        public async Task<bool> DeleteDeviceFromRoomAsync(Guid roomId, string deviceId, CancellationToken ct)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(deviceId))
                return false;

            var room = await DbContext.Rooms.Include(r => r.Devices).FirstOrDefaultAsync(r => r.Id == roomId, ct);
            if (room == null)
                return false;

            var device = room.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device == null)
                return false;

            room.Devices.Remove(device);
            await DbContext.SaveChangesAsync(ct);

            return true;
        }
    }
}
