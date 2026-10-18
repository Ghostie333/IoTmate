using AutoMapper;
using IoTmate.Common.Exceptions;
using IoTmate.Dto.Rule;
using IoTmate.Model.Entities;
using IoTmate.Repository.Devices;
using IoTmate.Repository.Rules;
using IoTmate.Services.Rooms;

namespace IoTmate.Services.Rules
{
    public class RuleService(
        IRuleRepository ruleRepository,
        IRoomService roomService,
        IDeviceRepository deviceRepository,
        IMapper mapper) : IRuleService
    {
        private readonly IRuleRepository _ruleRepository = ruleRepository;
        private readonly IRoomService _roomService = roomService;
        private readonly IDeviceRepository _deviceRepository = deviceRepository;
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
            newRule.RoomId = roomId;

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

            existingRule.Name = input.Name;
            existingRule.IsEnabled = input.IsEnabled;
            existingRule.TargetDeviceId = await ResolveDeviceIdAsync(input.TargetDeviceId, roomId, ct);
            existingRule.SourceDeviceId = await ResolveDeviceIdAsync(input.SourceDeviceId, roomId, ct);
            existingRule.TargetOutletNumber = input.TargetOutletNumber ?? 0;
            existingRule.Action = input.Action ?? default;
            existingRule.SensorFieldName = input.SensorFieldName;
            existingRule.Operation = input.Operation ?? default;
            existingRule.SensorRuleValue = input.SensorRuleValue ?? 0;
            existingRule.TimeHour = input.TimeHour;
            existingRule.TimeMinute = input.TimeMinute;
            existingRule.DaysOfWeek = input.DaysOfWeek ?? Array.Empty<bool>();

            await _ruleRepository.UpdateRuleAsync(existingRule, ct);

            var updatedWithRelations = await _ruleRepository.GetRuleByIdAsync(ruleId, ct);
            return _mapper.Map<RuleDto>(updatedWithRelations ?? existingRule);
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

        private async Task<Guid?> ResolveDeviceIdAsync(string? deviceIdMac, Guid roomId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(deviceIdMac))
                return null;

            var device = await _deviceRepository.GetByDeviceIdAsync(deviceIdMac, ct);

            if (device is null || device.RoomId != roomId)
                throw new NotFoundException("device.notFound");

            return device.Id;
        }
    }
}
