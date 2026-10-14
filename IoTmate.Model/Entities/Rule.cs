using IoTmate.Model.Enums;

namespace IoTmate.Model.Entities
{
    public class Rule
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public RuleType RuleType { get; set; } // Reading = 1, Time = 2
        public bool IsEnabled { get; set; } = true;
        public string? Name { get; set; }

        // Target for every RuleType
        // Device whose outlet is switched when the rule is true
        public Guid? TargetDeviceId { get; set; }
        public Device? TargetDevice { get; set; }
        public int TargetOutletNumber { get; set; } // outlet number on the target power strip
        public RuleAction Action { get; set; } // TurnOn = 1, TurnOff = 2

        // Reading trigger (RuleType == Reading)
        public Guid? SourceDeviceId { get; set; }
        public Device? SourceDevice { get; set; }
        public string? SensorFieldName { get; set; } // Field published by the source sensor ("temperature" etc.)
        public RuleOperation Operation { get; set; } // GreaterThan, GreaterThanOrEqual, etc.
        public float SensorRuleValue { get; set; }

        // Time trigger (RuleType == Time)
        public int? TimeHour { get; set; }
        public int? TimeMinute { get; set; }

        // For timed rules
        public bool[] DaysOfWeek { get; set; } = [];
    }
}