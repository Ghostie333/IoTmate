using System.Text;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace IoTmate.Infrastructure.Mqtt;

public sealed class MqttPublisher : IMqttPublisher
{
    private readonly MqttOptions _options;
    private readonly ILogger<MqttPublisher> _logger;

    public MqttPublisher(IOptions<MqttOptions> options, ILogger<MqttPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default)
    {
        var factory = new MqttFactory();
        using var client = factory.CreateMqttClient();

        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId($"iotmate-publisher-{Guid.NewGuid():N}")
            .Build();

        await client.ConnectAsync(clientOptions, cancellationToken);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .Build();

        await client.PublishAsync(message, cancellationToken);
        await client.DisconnectAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("MQTT published to {Topic}", topic);
    }
}
