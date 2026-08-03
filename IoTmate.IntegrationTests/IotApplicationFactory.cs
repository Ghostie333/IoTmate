using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace IoTmate.IntegrationTests
{
    public class IotApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer;
        private readonly IContainer _mqttContainer;

        // Minimalna konfiguracja Mosquitto pozwalająca na anonimowe połączenia testowe
        private const string MosquittoConf = @"
            listener 1883 0.0.0.0
            allow_anonymous true
            ";

        public IotApplicationFactory()
        {
            _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("iot_test_db")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            _mqttContainer = new ContainerBuilder("iegomez/mosquitto-go-auth")
                .WithPortBinding(1883, true)
                .WithResourceMapping(
                    System.Text.Encoding.UTF8.GetBytes(MosquittoConf),
                    "/etc/mosquitto/mosquitto.conf"
                )
                .Build();
        }

        public string MqttHost => _mqttContainer.Hostname;
        public int MqttPort => _mqttContainer.GetMappedPublicPort(1883);

        // Startujemy kontenery ZANIM konfiguracja WebHost odczyta porty!
        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            await _mqttContainer.StartAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Używamy AddInMemoryCollection zamiast Environment.SetEnvironmentVariable!
                // Dzięki temu nadpisujemy IConfiguration dynamicznymi portami po starcie kontenera.
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString()},
                    {"MqttSettings:Host", MqttHost},
                    {"MqttSettings:Port", MqttPort.ToString()},
                    {"MqttSettings:Username", "backend_app"},
                    {"MqttSettings:Password", "BackendPassword"}
                });
            });
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _mqttContainer.StopAsync();
        }
    }
}