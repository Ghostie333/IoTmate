using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTnet;

namespace IoTmate_backend.Services
{
    public class MqttBackgroundService() : BackgroundService
    {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly MqttClientFactory _mqttFactory;
        private readonly MqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;
        public MqttBackgroundService(ILogger<MqttBackgroundService> logger, MqttClientFactory mqttFactory, MqttClient mqttClient) : this()
        {
            _logger = logger;
            _mqttFactory = mqttFactory;
            _mqttClient = mqttClient;
            _mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("mqtt", 1883).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public async Task Connect()
        {
            var response = await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
            var mqttClientDisconnectOptions = _mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

            await _mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
            _logger.LogDebug("Connected to MQTT");
        }

        public async Task Disconnect()
        {
            await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
            await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
            _logger.LogDebug("Disconnected from MQTT");
        }

        public async Task SubscribeTopic(string topic)
        {
            await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);

            var topicFilter = _mqttFactory.CreateTopicFilterBuilder().WithTopic(topic).WithAtLeastOnceQoS();

            var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(topicFilter).Build();

            var response = await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            _logger.LogDebug($"MQTT client subscribed to topic: {topic}");
        }

        public async Task PublishMessage(string topic, string jsonMessage)
        {
            await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(jsonMessage)
                .Build();

            await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            await _mqttClient.DisconnectAsync();

            _logger.LogDebug($"MQTT message published to topic: {topic}");
        }
    }
}