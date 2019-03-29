using AutoMapper;
using HoaEntities.Entities;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using System;

namespace HoaWebAPI.AutoMapperProfiles
{
    public class BoardMeetingProfile : Profile
    {
        public BoardMeetingProfile()
        {
            CreateMap<BoardMeeting, BoardMeetingViewDto>()
                .ForMember(dest => dest.DaysOld, opt => opt.MapFrom(src => (DateTime.Now - src.Created).TotalDays))
                .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.CreatedBy.Email))
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}"))
                .ForMember(dest => dest.OwnerSocialId, opt => opt.MapFrom(src => src.CreatedBy.SocialId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.MeetingNotes.FileName))
                .ForMember(dest => dest.MeetingMinuteId, opt => opt.MapFrom(src => src.MeetingNotes.Id));

            CreateMap<BoardMeetingInputDto, BoardMeeting>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<BoardMeetingUpdateDto, BoardMeeting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<BoardMeeting, BoardMeetingUpdateDto>();
        }
    }
}
