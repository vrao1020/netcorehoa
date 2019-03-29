using AutoMapper;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.UpdateModels;

namespace HoaWebApplication.AutoMapperProfiles
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<PostManipulationDto, PostInputDto>();

            CreateMap<PostManipulationDto, PostUpdateDto>();
        }
    }
}
