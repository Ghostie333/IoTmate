using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace IoTmate.Services.Devices.StateHandlers
{
    public interface IDeviceStateHandler
    {
        DeviceType DeviceType { get; }
        Task HandleStatePayloadAsync(Device device, string payload, bool retained, IServiceScope scope, CancellationToken ct);
    }
}
