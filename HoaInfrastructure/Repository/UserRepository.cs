using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaInfrastructure.Context;
using HoaEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoaInfrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private HoaDbContext _context;

        public UserRepository(HoaDbContext hoaDbContextbContext)
        {
            _context = hoaDbContextbContext ?? throw new ArgumentNullException(nameof(hoaDbContextbContext));
        }

        //public async Task<IEnumerable<User>> GetUsersAsync(SieveModel sieveModel)
        //{
        //    //get max and default page values
        //    var maxPageSize = Int32.Parse(_configuration["Sieve:MaxPageSize"]);
        //    var defaultPageSize = Int32.Parse(_configuration["Sieve:DefaultPageSize"]);

        //    //set page size and number and check for null values as the library does not support default values
        //    var pageSize = sieveModel.PageSize ?? defaultPageSize;
        //    var pageNumber = sieveModel.Page ?? 1;
        //    pageSize = pageSize > maxPageSize ? defaultPageSize : pageSize;

        //    //fetch the events from the repo
        //    var users = _context.Users.AsNoTracking();

        //    //apply sorts and filters prior to getting total count for generating headers
        //    //this is so that headers don't return incorrect values due to filters applied
        //    //note that filters are applied first prior to sorts with Sieve.Apply
        //    users = _sieveProcessor.Apply(sieveModel, users, applyPagination: false);
        //    _paginationGenerator.GenerateHeaders(users.Count(), pageSize, pageNumber, sieveModel.Sorts, sieveModel.Filters);

        //    //apply the pagination and don't re-apply filtering and sorting as they have already been completed
        //    var usersToReturn = _sieveProcessor.Apply(sieveModel, users, applyFiltering: false, applySorting: false);

        //    return await usersToReturn.ToListAsync();
        //}

        public IQueryable<User> GetUsers()
        {
            return _context.Users.AsNoTracking();
        }

        public async Task<User> GetUserAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => id == x.Id);
        }

        public async Task<User> GetUserAsync(string socialId)
        {
            return await _context.Users.FirstOrDefaultAsync(x => socialId == x.SocialId);
        }

        public async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<Guid> ids)
        {
            return await _context.Users.Where(x => ids.Contains(x.Id))
                                 .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUsersWithRemindersAsync()
        {
            return await _context.Users.Where(x => x.Reminder == true)
                                 .Select(x => x.Email).ToListAsync();
        }

        public void AddUser(User userToAdd)
        {
            if (userToAdd == null)
            {
                throw new ArgumentNullException(nameof(userToAdd));
            }

            _context.Users.Add(userToAdd);
        }

        public void DeleteUser(User userToDelete)
        {
            if (userToDelete == null)
            {
                throw new ArgumentNullException(nameof(userToDelete));
            }

            _context.Users.Remove(userToDelete);
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
            return (await _context.SaveChangesAsync() >= 0);
        }

        public async Task<bool> UserExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(x => x.Id == id);
        }

        public void UpdateUser(User userToUpdate)
        {
            //nothing required
            //Mapping UserUpdateDto to User automatically makes the EF context track the object
            //The User will start being tracked and can be updated
        }

        public async Task<IEnumerable<Event>> GetUserEvents(Guid id)
        {
            //return await _context.Users.Include(x => x.Events)
            //                           .Where(x => x.Id == id)
            //                           .ToListAsync();

            return await _context.Events.Where(x => x.CreatedBy.Id == id)
                                         .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetUserPosts(Guid id)
        {
            return await _context.Posts.Where(x => x.CreatedBy.Id == id)
                                         .ToListAsync();
        }

        public async Task<IEnumerable<MeetingMinute>> GetUserMeetingMinutes(Guid id)
        {
            return await _context.MeetingMinutes.Where(x => x.CreatedBy.Id == id)
                                         .ToListAsync();
        }
    }
}
