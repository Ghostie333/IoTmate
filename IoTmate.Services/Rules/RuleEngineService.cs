using IoTmate.Common.TimeZones;
using IoTmate.Model.Entities;
using IoTmate.Model.Enums;
using IoTmate.Repository.Rules;
using IoTmate.Services.Devices;


namespace IoTmate.Services.Rules
{
    public class RuleEngineService(IRuleRepository ruleRepository, IDeviceStateService deviceStateService) : IRuleEngineService
    {
        private readonly IRuleRepository _ruleRepository = ruleRepository;
        private readonly IDeviceStateService _deviceStateService = deviceStateService;

        public async Task HandleReadingRuleAsync(Guid sourceDeviceId, Dictionary<string, float> readings, CancellationToken ct)
        {
            if (sourceDeviceId == Guid.Empty) return;
            List<Rule> rules = await _ruleRepository.GetEnabledReadingRulesAsync(sourceDeviceId, ct);
            if (rules.Count == 0) return;

            foreach (var rule in rules)
            {
                if (string.IsNullOrEmpty(rule.SensorFieldName) || rule.TargetDevice == null) continue;
                if (!readings.ContainsKey(rule.SensorFieldName)) continue;

                // Check if rule is true
                float value = readings[rule.SensorFieldName];
                bool isRuleTrue = rule.Operation switch
                {
                    RuleOperation.Equal => value == rule.SensorRuleValue,
                    RuleOperation.LessThan => value < rule.SensorRuleValue,
                    RuleOperation.LessThanOrEqual => value <= rule.SensorRuleValue,
                    RuleOperation.GreaterThan => value > rule.SensorRuleValue,
                    RuleOperation.GreaterThanOrEqual => value >= rule.SensorRuleValue,
                    _ => false
                };

                // Switch outlet state
                if (isRuleTrue)
                    await DispatchActionAsync(rule, ct);
            }
        }
        public async Task HandleTimeRuleAsync(DateTime utcNow, CancellationToken ct)
        {
            var rules = await _ruleRepository.GetEnabledTimeRulesAsync(ct);
            if (rules.Count == 0) return;

            foreach (var group in rules.GroupBy(rule => rule.Room?.TimeZoneId))
            {
                if (!TimeZoneHelper.TryGetLocalTime(utcNow, group.Key, out var localTime)) continue;

                foreach (var rule in group)
                {
                    if (rule.TargetDevice == null) continue;
                    if (rule.TimeHour == localTime.Hour && rule.TimeMinute == localTime.Minute && FiresOnDay(rule, localTime))
                        await DispatchActionAsync(rule, ct);
                }
            }
        }

        // Helpers
        private async Task DispatchActionAsync(Rule rule, CancellationToken ct)
        {
            if (rule.TargetDevice == null) return;
            if (rule.Action == RuleAction.TurnOn)
                await SetRelayStateAsync(rule.TargetDevice, rule.TargetOutletNumber, true, ct);
            else if (rule.Action == RuleAction.TurnOff)
                await SetRelayStateAsync(rule.TargetDevice, rule.TargetOutletNumber, false, ct);
        }
        private async Task SetRelayStateAsync(Device targetDevice, int outletNumber, bool turnOn, CancellationToken ct)
        {
            if (targetDevice == null || targetDevice.RelayState == null
                || outletNumber < 1 || targetDevice.RelayState.OutletsState.Length < outletNumber) return;

            // Send MQTT message to update outlet state only if new state is different than previous
            if (targetDevice.RelayState.OutletsState[outletNumber - 1] != turnOn)
                await _deviceStateService.SendRelayCommandAsync(targetDevice.DeviceId, outletNumber, turnOn, ct);
        }
        private static bool FiresOnDay(Rule rule, DateTime now)
        {
            if (rule.DaysOfWeek.Length != 7) return true; // not set = everyday
            int index = ((int)now.DayOfWeek + 6) % 7; // Monday = 0
            return rule.DaysOfWeek[index];
        }
    }
}
