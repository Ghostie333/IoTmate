using IoTmate.Dto.DeviceData;

namespace IoTmate.Services.Hubs
{
    public interface IDeviceHubClient
    {
        Task DeviceStatusChanged(DeviceStatusUpdateDto update);
        Task DeviceStateChanged(DeviceStateUpdateDto update);
        Task SensorReadingReceived(SensorReadingUpdateDto update);
    }
}
