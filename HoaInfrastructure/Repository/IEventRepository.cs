using HoaEntities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaInfrastructure.Repositories
{
    public interface IEventRepository
    {
        //Task<IEnumerable<Event>> GetEventsAsync(SieveModel sieveModel);
        IQueryable<Event> GetEvents();
        Task<Event> GetEventAsync(Guid eventId);
        Task<Event> GetCurrentDueEventAsync();
        void AddEvent(Event eventToAdd); //This not asynchronous because the object is added to the db context for tracking but its not saved yet.
                                         //So there is no need to make this async as saving the object will take care of the async operation
        void DeleteEvent(Event eventToDelete);
        Task<bool> SaveChangesAsync();
        Task<bool> EventExistsAsync(Guid id);
        void UpdateEvent(Event eventToUpdate);
        Task<IEnumerable<Event>> GetEventsAsync(IEnumerable<Guid> ids);
    }
}
