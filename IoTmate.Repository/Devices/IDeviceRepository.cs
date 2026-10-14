using IoTmate.Model.Entities;

namespace IoTmate.Repository.Devices
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByDeviceIdAsync(string deviceId, CancellationToken ct);
        Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken, CancellationToken ct);
        Task<bool> SaveDeviceAsync(Device device, CancellationToken ct);
    }
}
