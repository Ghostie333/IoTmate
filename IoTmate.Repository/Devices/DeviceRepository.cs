using IoTmate.Model;
using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IoTmate.Repository.Devices
{
    public class DeviceRepository(ApplicationDbContext dbContext) : BaseRepository(dbContext), IDeviceRepository
    {
        public async Task<bool> SaveDeviceAsync(Device device)
        {
            DbContext.Entry(device).State = device.Id == default ? EntityState.Added : EntityState.Modified;

            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<Device?> GetByDeviceIdAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return null;

            var normalizedDeviceId = deviceId.Trim().ToUpperInvariant();
            return await DbContext.Devices.SingleOrDefaultAsync(d => d.DeviceId == normalizedDeviceId);
        }

        public async Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken)
        {
            if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(secretToken))
                return false;

            var normalizedDeviceId = deviceId.Trim().ToUpperInvariant();

            var device = await DbContext.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeviceId == normalizedDeviceId);

            if (device == null || string.IsNullOrEmpty(device.SecretToken))
                return false;

            var incomingHash = HashToken(secretToken);

            var storedHashBytes = Encoding.UTF8.GetBytes(device.SecretToken);
            var incomingHashBytes = Encoding.UTF8.GetBytes(incomingHash);

            return CryptographicOperations.FixedTimeEquals(storedHashBytes, incomingHashBytes);
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexStringLower(bytes);
        }
    }
}