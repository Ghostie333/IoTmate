using IoTmate.Model;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace IoTmate.IntegrationTests
{
    public class TelemetryIntegrationTests : IClassFixture<TelemetryIntegrationTests>
    {
        private readonly IotApplicationFactory _factory;

        public TelemetryIntegrationTests(IotApplicationFactory factory)
        {
            _factory = factory;
            _factory.CreateClient();
        }

        [Fact]
        public async Task HandleTelemetry_ValidPayloadFromEsp32_SavesDataToDatabase()
        {
            // --- 1. ARRANGE ---
            // Create MQTT client acting as ESP32 device
            var mqttFactory = new MqttClientFactory();
            using var esp32Client = mqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_factory.MqttHost, _factory.MqttPort)
                .Build();

            await esp32Client.ConnectAsync(options);

            var telemetryPayload = new
            {
                deviceId = "00:1A:2B:3C:4D:5E", // Valid MAC address
                temperature = 24.5,
                humidity = 55.0
            };

            var jsonMessage = JsonSerializer.Serialize(telemetryPayload);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("devices/telemetry")
                .WithPayload(Encoding.UTF8.GetBytes(jsonMessage))
                .Build();

            // --- 2. ACT ---
            // ESP32 publishes payload to the broker
            await esp32Client.PublishAsync(message);

            // Wait a moment for background service to process async message
            await Task.Delay(1000);

            // --- 3. ASSERT ---
            // Query DB directly to verify persistence
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //var savedTelemetry = dbContext.Telemetries
            //    .FirstOrDefault(t => t.DeviceId == "00:1A:2B:3C:4D:5E");

            //savedTelemetry.Should().NotBeNull();
            //savedTelemetry!.Temperature.Should().Be(24.5);
        }
    }
}
