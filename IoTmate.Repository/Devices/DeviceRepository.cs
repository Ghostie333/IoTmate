using IoTmate.Common.Security;
using IoTmate.Model;
using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace IoTmate.Repository.Devices;

public class DeviceRepository(ApplicationDbContext dbContext) : BaseRepository(dbContext), IDeviceRepository
{
    public async Task<bool> SaveDeviceAsync(Device device, CancellationToken ct)
    {
        if (DbContext.Entry(device).State == EntityState.Detached)
        {
            DbContext.Devices.Update(device);
        }

        await DbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Device?> GetByDeviceIdAsync(string deviceId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return null;

        var normalizedDeviceId = deviceId.Trim().ToUpperInvariant();
        return await DbContext.Devices
            .Include(d => d.RelayState)
            .FirstOrDefaultAsync(d => d.DeviceId == normalizedDeviceId, ct);
    }

    public async Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(secretToken))
            return false;

        var normalizedDeviceId = deviceId.Trim().ToUpperInvariant();

        var device = await DbContext.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeviceId == normalizedDeviceId, ct);

        if (device == null || string.IsNullOrEmpty(device.SecretToken))
            return false;

        return CryptoUtils.FixedTimeEqualsHex(device.SecretToken, secretToken);
    }
}