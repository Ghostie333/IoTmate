namespace IoTmate.Infrastructure.Mqtt;

public interface IMqttPublisher
{
    Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default);
}
