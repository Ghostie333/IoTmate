using IoTmate.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoTmate.Services.Devices
{
    public class DevicePresenceSweepService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DevicePresenceSweepService> logger) : BackgroundService
    {
        private static readonly TimeSpan SweepInterval = TimeSpan.FromSeconds(30);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var offlineAfterSeconds = configuration.GetValue<int>("DevicePresence:OfflineAfterSeconds", 60);
            using var timer = new PeriodicTimer(SweepInterval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await SweepAsync(offlineAfterSeconds, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task SweepAsync(int offlineAfterSeconds, CancellationToken ct)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var cutoff = DateTime.UtcNow.AddSeconds(-offlineAfterSeconds);

                var affected = await dbContext.Devices
                    .Where(d => d.IsOnline && d.LastSeenAt < cutoff)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.IsOnline, false), ct);

                if (affected > 0)
                {
                    logger.LogInformation(
                        "Marked {Count} device(s) offline after {Seconds}s without telemetry.",
                        affected,
                        offlineAfterSeconds);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Device presence sweep failed.");
            }
        }
    }
}
