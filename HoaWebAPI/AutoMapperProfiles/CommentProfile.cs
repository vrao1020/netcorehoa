using AutoMapper;
using HoaEntities.Entities;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using System;

namespace HoaWebAPI.AutoMapperProfiles
{
    public class CommentProfile: Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentViewDto>()
                .ForMember(dest => dest.DaysOld, opt => opt.MapFrom(src => (DateTime.Now - src.Created).TotalDays))
                .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.CreatedBy.Email))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}"))
                .ForMember(dest => dest.OwnerSocialId, opt => opt.MapFrom(src => src.CreatedBy.SocialId));

            CreateMap<CommentInputDto, Comment>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.CommentForPost, opt => opt.Ignore());

            CreateMap<CommentUpdateDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())

                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCommentId, opt => opt.Ignore())
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.CommentForPost, opt => opt.Ignore());

            CreateMap<Comment, CommentUpdateDto>();
        }
    }
}
