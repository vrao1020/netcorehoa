using HoaInfrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HoaWebAPI.Extensions.ServiceConfiguration
{
    public static class RepositoryServices
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            //add repository services for CRUD operations
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IMeetingMinuteRepository, MeetingMinuteRepository>();
            services.AddScoped<IBoardMeetingRepository, BoardMeetingRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

            return services;
        }
    }
}
