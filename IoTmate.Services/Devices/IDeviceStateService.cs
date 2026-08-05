using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Services.Devices
{
    public interface IDeviceStateService
    {
        Task SendRelayCommandAsync(string deviceId, int relayNum, bool turnOn, CancellationToken ct);
        Task HandleStateAsync(string deviceId, string payload, CancellationToken ct);
        Task HandleStatusAsync(string deviceId, string payload, CancellationToken ct);
    }
}
