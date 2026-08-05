using IoTmate.Common.Security;
using IoTmate.Model;
using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace IoTmate.Repository.Devices;

public class DeviceRepository(ApplicationDbContext dbContext) : BaseRepository(dbContext), IDeviceRepository
{
    public async Task<bool> SaveDeviceAsync(Device device)
    {
        if (DbContext.Entry(device).State == EntityState.Detached)
        {
            DbContext.Devices.Update(device);
        }

        try
        {
            await DbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Device?> GetByDeviceIdAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return null;

        var normalizedDeviceId = deviceId.Trim().ToUpperInvariant();
        return await DbContext.Devices.FirstOrDefaultAsync(d => d.DeviceId == normalizedDeviceId);
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

        return CryptoUtils.FixedTimeEqualsHex(device.SecretToken, secretToken);
    }
}