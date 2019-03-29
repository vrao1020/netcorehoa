using AutoMapper;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;

namespace HoaWebApplication.AutoMapperProfiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<CommentManipulationDto, CommentInputDto>();

            CreateMap<CommentManipulationDto, CommentUpdateDto>();

            CreateMap<CommentViewDto, CommentManipulationDto>();
        }
    }
}
