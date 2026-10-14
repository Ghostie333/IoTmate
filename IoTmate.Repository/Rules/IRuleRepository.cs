using IoTmate.Model.Entities;

namespace IoTmate.Repository.Rules
{
    public interface IRuleRepository
    {
        // GET
        Task<Rule?> GetRuleByIdAsync(Guid id, CancellationToken ct);
        Task<List<Rule>> GetRulesByRoomIdAsync(Guid roomId, CancellationToken ct);
        Task<List<Rule>> GetEnabledReadingRulesAsync(Guid sourceDeviceId, CancellationToken ct); // For checking if sensor based rule is true
        Task<List<Rule>> GetEnabledTimeRulesAsync(CancellationToken ct); // Timed rules

        // CREATE / UPDATE
        Task<Rule?> CreateRuleAsync(Rule newRule, CancellationToken ct);
        Task UpdateRuleAsync(Rule updatedRule, CancellationToken ct);

        // DELETE
        Task<bool> DeleteRuleAsync(Guid id, CancellationToken ct);
    }
}
