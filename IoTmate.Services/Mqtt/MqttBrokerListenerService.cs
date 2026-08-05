using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace IoTmate.Services.Mqtt
{
    public class MqttBrokerListenerService : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttOptions;
        private readonly ILogger<MqttBrokerListenerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public MqttBrokerListenerService(
            IConfiguration configuration,
            ILogger<MqttBrokerListenerService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            var mqttHost = configuration["MqttSettings:Host"];
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

            // Po każdym (re)connect odtwarzamy subskrypcje
            _mqttClient.ConnectedAsync += OnConnectedAsync;

            // Tu ląduje każda wiadomość ze wszystkich subskrybowanych topiców
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation("Connected to MQTT Broker. Subscribing to topics...");

            // Wildcard "#" złapie WSZYSTKIE topiki na brokerze.
            await _mqttClient.SubscribeAsync(
                new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic("#"))
                    .Build());
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = args.ApplicationMessage.ConvertPayloadToString();

            _logger.LogInformation("Received message on {Topic}: {Payload}", topic, payload);

            try
            {
                // Scope, bo tu wchodzimy do logiki z DbContextem / serwisami scoped
                using var scope = _scopeFactory.CreateScope();
                var router = scope.ServiceProvider.GetRequiredService<IMqttMessageRouterService>();

                // POPRAWKA: Przekazujemy CancellationToken.None lub default
                await router.HandleAsync(topic, payload, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing MQTT message from topic {Topic}", topic);
            }
        }

        public async Task PublishAsync(string topic, string payload, CancellationToken ct = default)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.PublishAsync(message, ct);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(2000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Connecting to MQTT Broker...");
                    await _mqttClient.ConnectAsync(_mqttOptions, stoppingToken);
                    break;
                }
                catch (Exception)
                {
                    _logger.LogWarning("Could not connect to MQTT Broker. Retrying in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            // Trzymamy serwis żywy dopóki nie przyjdzie cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, stoppingToken);
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