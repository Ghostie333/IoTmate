using IoTmate.Model;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace IoTmate.Repository.Rooms
{
    public class RoomRepository(ApplicationDbContext dbContext) : BaseRepository(dbContext), IRoomRepository
    {
        // User rooms
        public async Task<Room?> GetRoomByIdAsync(Guid roomId)
        {
            if (roomId == Guid.Empty)
                return null;

            var room = await DbContext.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);

            return room;
        }
        public async Task<List<Room>> GetUserRoomsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return [];

            var rooms = await DbContext.Rooms.Where(r => r.Members.Any(u => u.UserId == userId)).ToListAsync();

            return rooms;
        }

        // Create, update, delete room
        public async Task<Room> CreateRoomAsync(Room room)
        {
            DbContext.Rooms.Add(room);
            await DbContext.SaveChangesAsync();
            return room;
        }
        public async Task<Room> CreateRoomWithOwnerAsync(Room room, string ownerUserId)
        {
            room.Members.Add(new RoomMember
            {
                RoomId = room.Id,
                UserId = ownerUserId,
                Role = RoomRole.Owner,
                JoinedAt = DateTime.UtcNow
            });

            DbContext.Rooms.Add(room);
            await DbContext.SaveChangesAsync();

            return room;
        }
        public async Task UpdateRoomAsync(Room room)
        {
            if (DbContext.Entry(room).State == EntityState.Detached)
                DbContext.Rooms.Update(room);

            await DbContext.SaveChangesAsync();
        }
        public async Task<bool> DeleteRoomAsync(Guid roomId)
        {
            if (roomId == Guid.Empty)
                return false;

            var room = await DbContext.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
                return false;

            DbContext.Rooms.Remove(room);
            await DbContext.SaveChangesAsync();

            return true;
        }

        // Room users
        public async Task<bool> AddUserToRoomAsync(Guid roomId, string userId, RoomRole roomRole)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(userId))
                return false;

            var userExists = await DbContext.Users.AnyAsync(u => u.Id == userId);
            var roomExists = await DbContext.Rooms.AnyAsync(r => r.Id == roomId);

            if (!userExists || !roomExists)
                return false;

            if (await DbContext.RoomMembers.AnyAsync(m => m.RoomId == roomId && m.UserId == userId))
                return false;

            RoomMember newRoomMember = new()
            {
                RoomId = roomId,
                UserId = userId,
                Role = roomRole,
                JoinedAt = DateTime.UtcNow
            };

            await DbContext.RoomMembers.AddAsync(newRoomMember);
            await DbContext.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteUserFromRoomAsync(Guid roomId, string userId)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(userId))
                return false;

            var room = await DbContext.Rooms.Include(r => r.Members).FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
                return false;

            var member = room.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
                return false;

            room.Members.Remove(member);
            await DbContext.SaveChangesAsync();

            return true;
        }
        public async Task<RoomRole?> GetUserRoleAsync(Guid roomId, string userId)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(userId))
                return null;

            var room = await DbContext.Rooms.Include(r => r.Members).FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
                return null;

            var member = room.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
                return null;

            return member.Role;
        }
        public async Task<List<RoomMember>> GetRoomMembersAsync(Guid roomId)
        {
            if (roomId == Guid.Empty)
                return [];

            var members = await DbContext.RoomMembers
                .Where(m => m.RoomId == roomId)
                .Include(m => m.User)
                .ToListAsync();

            return members;
        }

        // Devices
        public async Task<List<Device>> GetRoomDevicesAsync(Guid roomId)
        {
            if (roomId == Guid.Empty)
                return [];

            var room = await DbContext.Rooms.Include(r => r.Devices).FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
                return [];

            var roomDevices = room.Devices.ToList();

            return roomDevices;
        }
        public async Task<bool> AddDeviceToRoomAsync(Guid roomId, string deviceId)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(deviceId))
                return false;

            var updated = await DbContext.Devices
                .Where(d => d.DeviceId == deviceId && d.RoomId == null)
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.RoomId, roomId));

            return updated > 0;
        }
        public async Task<bool> DeleteDeviceFromRoomAsync(Guid roomId, string deviceId)
        {
            if (roomId == Guid.Empty || string.IsNullOrEmpty(deviceId))
                return false;

            var room = await DbContext.Rooms.Include(r => r.Devices).FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
                return false;

            var device = room.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device == null)
                return false;

            room.Devices.Remove(device);
            await DbContext.SaveChangesAsync();

            return true;
        }
    }
}
