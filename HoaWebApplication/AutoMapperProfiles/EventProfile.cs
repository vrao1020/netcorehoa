using AutoMapper;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.UpdateModels;

namespace HoaWebApplication.AutoMapperProfiles
{
    public class EventProfile: Profile
    {
        public EventProfile()
        {
            CreateMap<EventManipulationDto, EventInputDto>();

            CreateMap<EventManipulationDto, EventUpdateDto>();
        }
    }
}
