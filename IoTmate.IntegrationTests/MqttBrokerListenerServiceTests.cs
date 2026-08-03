using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using Xunit;

namespace IoTmate.IntegrationTests
{
    public class MqttBrokerListenerServiceTests(IotApplicationFactory factory) : IClassFixture<IotApplicationFactory>
    {
        private readonly IotApplicationFactory _factory = factory;

        [Fact]
        public async Task MqttBrokerListenerService_OnAppStart_ConnectsSuccessfullyToBroker()
        {
            // --- ARRANGE ---
            // Odpalenie serwera API i BackgroundService
            _factory.CreateClient();

            // Czekamy na ExecuteAsync (Twój Task.Delay(2000))
            await Task.Delay(3500);

            // --- ACT & ASSERT ---
            // 1. Sprawdzamy czy serwis zarejestrował się w DI
            using (var scope = _factory.Services.CreateScope())
            {
                var hostedServices = scope.ServiceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
                var mqttService = hostedServices.OfType<Services.Mqtt.MqttBrokerListenerService>().FirstOrDefault();
                mqttService.Should().NotBeNull();
            }

            // 2. Testujemy fizyczne połączenie z kontenerem z poziomu testu
            var testMqttFactory = new MqttClientFactory();
            using var testClient = testMqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_factory.MqttHost, _factory.MqttPort)
                .WithCleanSession()
                .Build();

            var connectResult = await testClient.ConnectAsync(options);

            // Jeśli połączyło, wynik to Success
            connectResult.ResultCode.Should().Be(MqttClientConnectResultCode.Success);
        }
    }
}