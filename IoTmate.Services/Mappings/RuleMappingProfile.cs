using AutoMapper;
using IoTmate.Dto.Rule;
using IoTmate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Services.Mappings
{
    public class RuleMappingProfile : Profile
    {
        public RuleMappingProfile()
        {
            CreateMap<CreateRuleInputDto, Rule>();
            CreateMap<UpdateRuleInputDto, Rule>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Ignoruje null-y przy patchowaniu/aktualizacji
            CreateMap<Rule, RuleDto>();
        }
    }
}
