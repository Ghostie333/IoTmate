using IoTmate.Model.Entities;

namespace IoTmate.Repository.Devices
{
    public interface IDeviceRepository
    {
        Task<Device?> GetByDeviceIdAsync(string deviceId);
        Task<bool> ValidateCredentialsAsync(string deviceId, string secretToken);
        Task<bool> SaveDeviceAsync(Device device);
    }
}
