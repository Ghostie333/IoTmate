using AutoMapper;
using IoTmate.Dto.Rule;
using IoTmate.Model.Entities;

namespace IoTmate.Services.Mappings
{
    public class RuleMappingProfile : Profile
    {
        public RuleMappingProfile()
        {
            CreateMap<CreateRuleInputDto, Rule>()
                .ForMember(dest => dest.TargetDeviceId, opt => opt.Ignore())
                .ForMember(dest => dest.SourceDeviceId, opt => opt.Ignore());

            CreateMap<Rule, RuleDto>()
                .ForMember(dest => dest.TargetDeviceId, opt => opt.MapFrom(src => src.TargetDevice != null ? src.TargetDevice.DeviceId : null))
                .ForMember(dest => dest.SourceDeviceId, opt => opt.MapFrom(src => src.SourceDevice != null ? src.SourceDevice.DeviceId : null));
        }
    }
}
