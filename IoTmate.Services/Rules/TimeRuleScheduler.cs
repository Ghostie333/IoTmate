using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoTmate.Services.Rules
{
    public class TimeRuleScheduler(
        IServiceScopeFactory scopeFactory,
        ILogger<TimeRuleScheduler> logger) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var ruleEngine = scope.ServiceProvider.GetRequiredService<IRuleEngineService>();
                    await ruleEngine.HandleTimeRuleAsync(DateTime.UtcNow, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while evaluating time rules");
                }
            }
        }
    }
}