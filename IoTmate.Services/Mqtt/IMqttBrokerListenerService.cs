using MQTTnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Services.Mqtt
{
    public interface IMqttBrokerListenerService
    {
        Task OnConnectedAsync(MqttClientConnectedEventArgs args);
        Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args);
        Task PublishAsync(string topic, string payload, CancellationToken ct = default);
        Task ExecuteAsync(CancellationToken stoppingToken);
        Task StopAsync(CancellationToken cancellationToken);

    }
}
