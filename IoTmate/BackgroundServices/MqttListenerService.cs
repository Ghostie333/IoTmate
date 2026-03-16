using System.Text;
using System.Text.Json;
using IoTmate.Contracts.Telemetry;
using IoTmate.Infrastructure.Mqtt;
using IoTmate.Services;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace IoTmate.BackgroundServices;

public sealed class MqttListenerService : BackgroundService
{
    private readonly ILogger<MqttListenerService> _logger;
    private readonly ITelemetryBus _telemetry;
    private readonly IDeviceStore _devices;
    private readonly MqttOptions _options;

    public MqttListenerService(
        ILogger<MqttListenerService> logger,
        ITelemetryBus telemetry,
        IDeviceStore devices,
        IOptions<MqttOptions> options)
    {
        _logger = logger;
        _telemetry = telemetry;
        _devices = devices;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        using var client = factory.CreateMqttClient();

        client.ApplicationMessageReceivedAsync += e =>
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = e.ApplicationMessage.PayloadSegment;
                var json = payload.Count == 0 ? null : Encoding.UTF8.GetString(payload);

                // Oczekiwany topic: devices/{deviceId}/telemetry/{key}
                // payload: { "value": 12.3 } albo samo 12.3
                if (!TryParseTelemetry(topic, json, out var point))
                {
                    _logger.LogDebug("MQTT message ignored. Topic={Topic}", topic);
                    return Task.CompletedTask;
                }

                _telemetry.Publish(point);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed handling MQTT message");
            }

            return Task.CompletedTask;
        };

        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId($"iotmate-backend-{Guid.NewGuid():N}")
            .Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!client.IsConnected)
                {
                    _logger.LogInformation("Connecting to MQTT broker {Host}:{Port}...", _options.Host, _options.Port);
                    await client.ConnectAsync(clientOptions, stoppingToken);

                    _logger.LogInformation("Connected. Subscribing to {Topic}", _options.TelemetryTopic);
                    var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(f => f.WithTopic(_options.TelemetryTopic))
                        .Build();

                    await client.SubscribeAsync(subscribeOptions, stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MQTT connection loop error. Retrying...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        try
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(cancellationToken: stoppingToken);
            }
        }
        catch
        {
            // ignore shutdown exceptions
        }
    }

    private static bool TryParseTelemetry(string topic, string? payloadJson, out TelemetryPointDto point)
    {
        point = default!;

        var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4) return false;

        if (!parts[0].Equals("devices", StringComparison.OrdinalIgnoreCase)) return false;
        var deviceId = parts[1];
        if (!parts[2].Equals("telemetry", StringComparison.OrdinalIgnoreCase)) return false;
        var key = string.Join('/', parts.Skip(3));

        var now = DateTimeOffset.UtcNow;

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return false;
        }

        if (double.TryParse(payloadJson, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var rawNumber))
        {
            point = new TelemetryPointDto(deviceId, key, rawNumber, now);
            return true;
        }

        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("value", out var valueEl) &&
                valueEl.TryGetDouble(out var value))
            {
                point = new TelemetryPointDto(deviceId, key, value, now);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
