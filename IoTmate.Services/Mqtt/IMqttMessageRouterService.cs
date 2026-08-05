using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Services.Mqtt
{
    public interface IMqttMessageRouterService
    {
        Task HandleAsync(string topic, string payload, CancellationToken ct);
    }
}
