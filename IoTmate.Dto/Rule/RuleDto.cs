using IoTmate.Model.Enums;

namespace IoTmate.Dto.Rule
{
    public record RuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public RuleType RuleType { get; set; } // Reading = 1, Time = 2
        public Guid? TargetDeviceId { get; set; }
        public int? TargetOutletNumber { get; set; }
        public RuleAction? Action { get; set; } // TurnOn = 1, TurnOff = 2
        public Guid? SourceDeviceId { get; set; }
        public string? SensorFieldName { get; set; }
        public RuleOperation? Operation { get; set; }
        public float? SensorRuleValue { get; set; }
        public int? TimeHour { get; set; }
        public int? TimeMinute { get; set; }
        public bool[]? DaysOfWeek { get; set; }
    }
}
