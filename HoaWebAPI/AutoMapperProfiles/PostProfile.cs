using AutoMapper;
using HoaEntities.Entities;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using System;

namespace HoaWebAPI.AutoMapperProfiles
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, PostViewDto>()
                .ForMember(dest => dest.DaysOld, opt => opt.MapFrom(src => (DateTime.Now - src.Created).TotalDays))
                .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.CreatedBy.Email))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}"))
                .ForMember(dest => dest.NumberOfComments, opt => opt.MapFrom(src => src.Comments.Count))
                .ForMember(dest => dest.OwnerSocialId, opt => opt.MapFrom(src => src.CreatedBy.SocialId));

            CreateMap<PostInputDto, Post>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            CreateMap<PostUpdateDto, Post>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            CreateMap<Post, PostUpdateDto>();
        }
    }
}
