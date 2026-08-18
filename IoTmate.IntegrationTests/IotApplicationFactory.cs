using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
            builder.UseSetting("ConnectionStrings:DefaultConnection", _dbContainer.GetConnectionString());
            builder.UseSetting("MqttSettings:Host", MqttHost);
            builder.UseSetting("MqttSettings:Port", MqttPort.ToString());
            builder.UseSetting("MqttSettings:Username", "backend_app");
            builder.UseSetting("MqttSettings:Password", "BackendPassword");
            builder.UseSetting("MqttSettings:UseTls", "false");
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _mqttContainer.StopAsync();
        }
    }
}