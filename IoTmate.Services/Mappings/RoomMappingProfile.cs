using AutoMapper;
using IoTmate.Dto.Device;
using IoTmate.Dto.Room;
using IoTmate.Model.Entities;

namespace IoTmate.Services.Mappings
{
    public class RoomMappingProfile : Profile
    {
        public RoomMappingProfile()
        {
            CreateMap<CreateRoomInputDto, Room>();
            CreateMap<Room, RoomDto>();
            CreateMap<Device, DeviceDto>()
                .ForMember(dest => dest.OutletsState, opt => opt.MapFrom(src =>
                    src.RelayState != null ? src.RelayState.OutletsState : null));
        }
    }
}