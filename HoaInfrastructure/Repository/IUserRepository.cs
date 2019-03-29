using HoaEntities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaInfrastructure.Repositories
{
    public interface IUserRepository
    {
        //Task<IEnumerable<User>> GetUsersAsync(SieveModel sieveModel);
        IQueryable<User> GetUsers();
        Task<User> GetUserAsync(Guid id);
        Task<User> GetUserAsync(string socialId);
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<Guid> ids);
        Task<IEnumerable<string>> GetUsersWithRemindersAsync();
        void AddUser(User userToAdd); //This not asynchronous because the object is added to the db context for tracking but its not saved yet.
                                      //So there is no need to make this async as saving the object will take care of the async operation
        void DeleteUser(User userToDelete);
        Task<bool> SaveChangesAsync();
        Task<bool> UserExistsAsync(Guid id);
        void UpdateUser(User userToUpdate);
        Task<IEnumerable<Event>> GetUserEvents(Guid id);
        Task<IEnumerable<Post>> GetUserPosts(Guid id);
        Task<IEnumerable<MeetingMinute>> GetUserMeetingMinutes(Guid id);
    }
}
