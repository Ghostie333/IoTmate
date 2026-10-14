using IoTmate.Dto.Rule;

namespace IoTmate.Services.Rules
{
    public interface IRuleService
    {
        Task<List<RuleDto>> GetRulesByRoomId(Guid roomId, string currentUserId, CancellationToken ct);
        Task<RuleDto> CreateRule(CreateRuleInputDto ruleDto, Guid roomId, string currentUserId, CancellationToken ct);
        Task<RuleDto> UpdateRule(UpdateRuleInputDto ruleDto, Guid roomId, Guid ruleId, string currentUserId, CancellationToken ct);
        Task<bool> DeleteRule(Guid ruleId, Guid roomId, string currentUserId, CancellationToken ct);
    }
}
