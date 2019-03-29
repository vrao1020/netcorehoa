using HoaEntities.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace HoaWebAPI.Extensions.Sieve
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(IOptions<SieveOptions> options,
           ISieveCustomFilterMethods customFilterMethods) : base(options, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            //events property sorting/filtering
            mapper.Property<Event>(p => p.Title)
                 .CanFilter()
                 .CanSort();

            mapper.Property<Event>(p => p.Message)
                 .CanSort()
                 .CanFilter();

            //posts property sorting/filtering
            mapper.Property<Post>(post => post.Title)
                 .CanFilter()
                 .CanSort();

            mapper.Property<Post>(post => post.Important)
                 .CanFilter()
                 .CanSort();

            mapper.Property<Post>(post => post.Message)
                 .CanFilter()
                 .CanSort();

            mapper.Property<MeetingMinute>(p => p.FileName)
                 .CanFilter();

            //users property sorting/filtering
            mapper.Property<User>(p => p.LastName)
                 .CanFilter()
                 .CanSort();

            mapper.Property<User>(p => p.FirstName)
                 .CanFilter()
                 .CanSort();

            mapper.Property<User>(p => p.Email)
                 .CanFilter()
                 .CanSort();

            mapper.Property<User>(p => p.Reminder)
                 .CanFilter()
                 .CanSort();

            //Board meetings property sorting/filtering
            mapper.Property<BoardMeeting>(p => p.Title)
                 .CanFilter()
                 .CanSort();

            mapper.Property<BoardMeeting>(p => p.Description)
                 .CanFilter()
                 .CanSort();

            //Comments property sorting/filtering
            mapper.Property<Comment>(p => p.Message)
                 .CanFilter()
                 .CanSort();

            return mapper;
        }
    }
}
