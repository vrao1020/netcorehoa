using HoaEntities.Entities;
using HoaWebAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HoaWebAPI.Extensions.ServiceConfiguration
{
    public static class SortFilterServices
    {
        public static IServiceCollection AddSortingFiltering(this IServiceCollection services)
        {
            //add sort/filter services
            services.AddScoped<ISortFilterService<BoardMeeting>, SortFilterService<BoardMeeting>>();
            services.AddScoped<ISortFilterService<Comment>, SortFilterService<Comment>>();
            services.AddScoped<ISortFilterService<Event>, SortFilterService<Event>>();
            services.AddScoped<ISortFilterService<MeetingMinute>, SortFilterService<MeetingMinute>>();
            services.AddScoped<ISortFilterService<Post>, SortFilterService<Post>>();
            services.AddScoped<ISortFilterService<User>, SortFilterService<User>>();

            return services;
        }
    }
}
