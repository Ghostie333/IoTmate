using IoTmate.Model.Enums;

namespace IoTmate.Model.Entities
{
    public class Rule
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public RuleType RuleType { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? Name { get; set; }

        // Target (shared by every rule type)
        // Device whose outlet is switched when the rule fires
        public Guid? TargetDeviceId { get; set; }
        public Device? TargetDevice { get; set; }
        // 1-based outlet number on the target power strip
        public int TargetOutletNumber { get; set; }
        public RuleAction Action { get; set; }

        // Reading trigger (RuleType == Reading)
        public Guid? SourceDeviceId { get; set; }
        public Device? SourceDevice { get; set; }
        // Field published by the source sensor ("temperature")
        public string? SensorFieldName { get; set; }
        public RuleOperation Operation { get; set; }
        public float SensorRuleValue { get; set; }

        // Time trigger (RuleType == Time)
        public int? TimeHour { get; set; }
        public int? TimeMinute { get; set; }

        // For timed rules
        // public int? DaysOfWeekMask { get; set; }
    }
}