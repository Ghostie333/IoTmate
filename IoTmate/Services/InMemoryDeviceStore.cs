using System.Collections.Concurrent;
using IoTmate.Contracts.Devices;

namespace IoTmate.Services;

public sealed class InMemoryDeviceStore : IDeviceStore
{
    private readonly ConcurrentDictionary<(string OwnerUserId, string DeviceId), DeviceDto> _devices = new();

    public InMemoryDeviceStore()
    {
        // empty by default; devices are created after user login/provisioning
    }

    public IReadOnlyList<DeviceDto> GetAll(string ownerUserId) =>
        _devices
            .Where(kvp => kvp.Key.OwnerUserId.Equals(ownerUserId, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Value)
            .OrderBy(d => d.Id)
            .ToArray();

    public DeviceDto? GetById(string ownerUserId, string id)
        => _devices.TryGetValue((ownerUserId, id), out var device) ? device : null;

    public DeviceDto Upsert(string ownerUserId, DeviceDto device)
    {
        _devices.AddOrUpdate((ownerUserId, device.Id), device, (_, _) => device);
        return device;
    }
}
