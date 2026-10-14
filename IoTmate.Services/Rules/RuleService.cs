using AutoMapper;
using IoTmate.Common.Exceptions;
using IoTmate.Dto.Rule;
using IoTmate.Model.Entities;
using IoTmate.Repository.Rules;
using IoTmate.Services.Rooms;

namespace IoTmate.Services.Rules
{
    public class RuleService(
        IRuleRepository ruleRepository,
        IRoomService roomService,
        IMapper mapper) : IRuleService
    {
        private readonly IRuleRepository _ruleRepository = ruleRepository;
        private readonly IRoomService _roomService = roomService;
        private readonly IMapper _mapper = mapper;

        public async Task<List<RuleDto>> GetRulesByRoomId(Guid roomId, string currentUserId, CancellationToken ct)
        {
            await _roomService.GetUserRole(roomId, currentUserId, ct);

            var rules = await _ruleRepository.GetRulesByRoomIdAsync(roomId, ct);
            return _mapper.Map<List<RuleDto>>(rules);
        }

        public async Task<RuleDto> CreateRule(CreateRuleInputDto input, Guid roomId, string currentUserId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(input);
            await _roomService.GetUserRole(roomId, currentUserId, ct);

            var newRule = _mapper.Map<Rule>(input);
            newRule.RoomId = roomId; // KLUCZOWE: przypisanie ID pokoju z route'a

            var createdRule = await _ruleRepository.CreateRuleAsync(newRule, ct);
            return _mapper.Map<RuleDto>(createdRule);
        }

        public async Task<RuleDto> UpdateRule(UpdateRuleInputDto input, Guid roomId, Guid ruleId, string currentUserId, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(input);
            await _roomService.GetUserRole(roomId, currentUserId, ct);

            var existingRule = await _ruleRepository.GetRuleByIdAsync(ruleId, ct)
                ?? throw new NotFoundException("Rule not found.");

            if (existingRule.RoomId != roomId)
                throw new NotFoundException("Rule not found in this room.");

            _mapper.Map(input, existingRule);
            await _ruleRepository.UpdateRuleAsync(existingRule, ct);

            return _mapper.Map<RuleDto>(existingRule);
        }

        public async Task<bool> DeleteRule(Guid ruleId, Guid roomId, string currentUserId, CancellationToken ct)
        {
            await _roomService.GetUserRole(roomId, currentUserId, ct);

            var existingRule = await _ruleRepository.GetRuleByIdAsync(ruleId, ct)
                ?? throw new NotFoundException("Rule not found.");

            if (existingRule.RoomId != roomId)
                throw new NotFoundException("Rule not found in this room.");

            return await _ruleRepository.DeleteRuleAsync(ruleId, ct);
        }
    }
}