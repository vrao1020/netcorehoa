using AutoMapper;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;

namespace HoaWebApplication.AutoMapperProfiles
{
    public class BoardMeetingProfile : Profile
    {
        public BoardMeetingProfile()
        {
            CreateMap<BoardMeetingManipulationDto, BoardMeetingInputDto>();

            CreateMap<BoardMeetingManipulationDto, BoardMeetingUpdateDto>();

            CreateMap<BoardMeetingViewDto, BoardMeetingManipulationDto>();
        }
    }
}
