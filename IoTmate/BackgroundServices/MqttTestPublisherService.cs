using System.Text.Json;
using IoTmate.Infrastructure.Mqtt;
using MQTTnet;
using MQTTnet.Client;

namespace IoTmate.BackgroundServices;

public sealed class MqttTestPublisherService : BackgroundService
{
    private readonly ILogger<MqttTestPublisherService> _logger;
    private readonly MqttOptions _options;

    public MqttTestPublisherService(ILogger<MqttTestPublisherService> logger, Microsoft.Extensions.Options.IOptions<MqttOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Publikuje testowe dane co 5s dla devices/test-device.
        var factory = new MqttFactory();
        using var client = factory.CreateMqttClient();

        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId($"iotmate-test-publisher-{Guid.NewGuid():N}")
            .Build();

        await client.ConnectAsync(clientOptions, stoppingToken);

        var rand = new Random();
        while (!stoppingToken.IsCancellationRequested)
        {
            var temp = Math.Round(20 + rand.NextDouble() * 8, 2);
            var hum = Math.Round(40 + rand.NextDouble() * 30, 2);

            await PublishAsync(client, "devices/test-device/telemetry/temp", new { value = temp }, stoppingToken);
            await PublishAsync(client, "devices/test-device/telemetry/humidity", new { value = hum }, stoppingToken);

            _logger.LogInformation("Published test telemetry for test-device");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        try { await client.DisconnectAsync(cancellationToken: stoppingToken); } catch { }
    }

    private static Task PublishAsync(IMqttClient client, string topic, object payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .Build();

        return client.PublishAsync(message, ct);
    }
}
