namespace IoTmate.Services.Rules
{
    // Service for handling rules operations
    public interface IRuleEngineService
    {
        Task HandleReadingRuleAsync(Guid sourceDeviceId, Dictionary<string, float> readings, CancellationToken ct);
        Task HandleTimeRuleAsync(DateTime utcNow, CancellationToken ct);
    }
}
