using FluentAssertions;
using IoTmate.Model;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using System.Text;
using System.Text.Json;

namespace IoTmate.IntegrationTests
{
    public class TelemetryIntegrationTests(IotApplicationFactory factory) : IClassFixture<IotApplicationFactory>
    {
        private readonly IotApplicationFactory _factory = factory;

        [Fact]
        public async Task HandleState_SensorPayloadFromEsp32_SavesReadingToDatabase()
        {
            _factory.CreateClient();

            const string deviceId = "00:1A:2B:3C:4D:5E";

            using (var seedScope = _factory.Services.CreateScope())
            {
                var seedDbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                seedDbContext.Devices.Add(new Device
                {
                    DeviceId = deviceId,
                    DeviceSecret = "secret",
                    SecretToken = "token",
                    Name = "TestSensor",
                    DeviceType = DeviceType.Sensor
                });
                await seedDbContext.SaveChangesAsync();
            }

            // Wait for the MQTT listener to connect to the broker
            await Task.Delay(3500);

            var mqttFactory = new MqttClientFactory();
            using var esp32Client = mqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_factory.MqttHost, _factory.MqttPort)
                .Build();
            await esp32Client.ConnectAsync(options);

            var payload = JsonSerializer.Serialize(new { temperature = 24.5, humidity = 55.0 });
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"devices/{deviceId}/state")
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .Build();

            await esp32Client.PublishAsync(message);

            // Wait for the background service to process the message
            await Task.Delay(1500);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedDevice = dbContext.Devices
                .Include(d => d.SensorReadings)
                .Single(d => d.DeviceId == deviceId);

            savedDevice.LastReading.Should().NotBeNull();
            savedDevice.LastReading!.Should().ContainKey("temperature").WhoseValue.Should().Be(24.5f);
            savedDevice.LastReading!["humidity"].Should().Be(55.0f);
            savedDevice.SensorReadings.Should().ContainSingle();
        }
    }
}