using AutoMapper;
using HoaEntities.Entities;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using System;

namespace HoaWebAPI.AutoMapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserViewDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<UserInputDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingMinutes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.BoardMeetings, opt => opt.Ignore())
                .ForMember(dest => dest.Joined, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Active, opt => opt.Ignore())
                .ForMember(dest => dest.SocialId, opt => opt.Ignore()); //this needs to be manually mapped 
                                                                        //in the controller

            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore())
                .ForMember(dest => dest.MeetingMinutes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.BoardMeetings, opt => opt.Ignore())
                .ForMember(dest => dest.Joined, opt => opt.Ignore())
                .ForMember(dest => dest.Active, opt => opt.Ignore())
                .ForMember(dest => dest.SocialId, opt => opt.Ignore()); //this needs to be manually mapped 
                                                                        //in the controller

            CreateMap<User, UserUpdateDto>();
        }
    }
}
