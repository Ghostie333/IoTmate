using IoTmate.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace IoTmate.Dto.Rule
{
    public record CreateRuleInputDto
    {
        [Required(AllowEmptyStrings = false)]
        public required string Name { get; set; }
        [Range(1, 2)]
        public required RuleType RuleType { get; set; }
    }
}
