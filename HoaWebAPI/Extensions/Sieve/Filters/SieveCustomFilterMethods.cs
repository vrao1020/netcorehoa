using HoaEntities.Entities;
using Sieve.Services;
using System.Linq;

namespace HoaWebAPI.Extensions.Sieve.Filters
{//NEED TO MODIFY THIS FOR MULTIPLE FILTER VALUES - LOOK UP DOCUMENTATION
    public class SieveCustomFilterMethods: ISieveCustomFilterMethods
    {
        public IQueryable<Event> OwnerEmail(IQueryable<Event> source, string op, string[] values)
        {
            return source.Where(p => p.CreatedBy.Email.Contains(values[0]));
        }

        public IQueryable<Post> OwnerEmail(IQueryable<Post> source, string op, string[] values)
        {
            return source.Where(p => p.CreatedBy.Email.Contains(values[0]));
        }

        public IQueryable<MeetingMinute> OwnerEmail(IQueryable<MeetingMinute> source, string op, string[] values)
        {
            return source.Where(p => p.CreatedBy.Email.Contains(values[0]));
        }


        public IQueryable<BoardMeeting> OwnerEmail(IQueryable<BoardMeeting> source, string op, string[] values)
        {
            return source.Where(p => p.CreatedBy.Email.Contains(values[0]));
        }

        public IQueryable<User> FullName(IQueryable<User> source, string op, string[] values)
        {
            return source.Where(p => p.FirstName.Contains(values[0]) || p.LastName.Contains(values[0]));
        }

        public IQueryable<Comment> OwnerEmail(IQueryable<Comment> source, string op, string[] values)
        {
            return source.Where(p => p.CreatedBy.Email.Contains(values[0]));
        }

        //public IQueryable<Event> DaysOld(IQueryable<Event> source, string op, string value)
        //{
        //    if(op == ">")
        //    {
        //        return source.Where(p => (DateTime.Now - p.Created).TotalDays >= Int32.Parse(value));
        //    }
        //    else if (op == "<")
        //    {
        //        return source.Where(p => (DateTime.Now - p.Created).TotalDays <= Int32.Parse(value));
        //    }
        //    else
        //    {
        //        return source;
        //    }
        //}
    }
}
