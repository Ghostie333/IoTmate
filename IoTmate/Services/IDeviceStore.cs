using IoTmate.Contracts.Devices;

namespace IoTmate.Services;

public interface IDeviceStore
{
    IReadOnlyList<DeviceDto> GetAll(string ownerUserId);
    DeviceDto? GetById(string ownerUserId, string id);
    DeviceDto Upsert(string ownerUserId, DeviceDto device);
}
