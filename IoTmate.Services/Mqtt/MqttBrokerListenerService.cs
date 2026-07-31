using MQTTnet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace IoTmate.Services.Mqtt
{
    public class MqttBrokerListenerService : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        private readonly ILogger<MqttBrokerListenerService> _logger;

        public MqttBrokerListenerService(
            IConfiguration configuration,
            ILogger<MqttBrokerListenerService> logger)
        {
            _logger = logger;
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            // Settings from appsettings.json
            var mqttHost = configuration["MqttSettings:Host"] ?? "mosquitto";
            var mqttPort = configuration.GetValue<int>("MqttSettings:Port", 1883);
            var username = configuration["MqttSettings:Username"];
            var password = configuration["MqttSettings:Password"];
            var clientId = configuration["MqttSettings:ClientId"] ?? $"backend_{Guid.NewGuid():N}";

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttHost, mqttPort)
                .WithCredentials(username, password)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        _logger.LogInformation("Connecting to MQTT Broker at {Host}:{Port}...",
                            _mqttOptions.ChannelOptions, 1883);

                        await _mqttClient.ConnectAsync(_mqttOptions, stoppingToken);

                        _logger.LogInformation("Successfully connected to MQTT Broker!");

                        await _mqttClient.SubscribeAsync("devices/+/telemetry", cancellationToken: stoppingToken);
                    }

                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Could not connect to MQTT Broker. Retrying in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}