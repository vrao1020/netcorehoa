using AutoMapper;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.UpdateModels;

namespace HoaWebApplication.AutoMapperProfiles
{
    public class MeetingMinuteProfile : Profile
    {
        public MeetingMinuteProfile()
        {
            CreateMap<MeetingMinuteManipulationDto, MeetingMinuteInputDto>();

            CreateMap<MeetingMinuteManipulationDto, MeetingMinuteUpdateDto>();
        }
    }
}
