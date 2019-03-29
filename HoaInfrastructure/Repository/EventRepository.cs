using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaInfrastructure.Context;
using HoaEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoaInfrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private HoaDbContext _context;

        public EventRepository(HoaDbContext hoaDbContextbContext)
        {
            _context = hoaDbContextbContext ?? throw new ArgumentNullException(nameof(hoaDbContextbContext));
        }

        public async Task<Event> GetEventAsync(Guid eventId)
        {
            var eventToReturn = await _context.Events.Include(s => s.CreatedBy)
                                              .FirstOrDefaultAsync(x => x.Id == eventId);
            return eventToReturn;
        }

        //public async Task<IEnumerable<Event>> GetEventsAsync(SieveModel sieveModel)
        //{
        //    //get max and default page values
        //    var maxPageSize = Int32.Parse(_configuration["Sieve:MaxPageSize"]);
        //    var defaultPageSize = Int32.Parse(_configuration["Sieve:DefaultPageSize"]);

        //    //set page size and number and check for null values as the library does not support default values
        //    var pageSize = sieveModel.PageSize ?? defaultPageSize;
        //    var pageNumber = sieveModel.Page ?? 1;
        //    pageSize = pageSize > maxPageSize ? defaultPageSize : pageSize;

        //    //fetch the events from the repo
        //    var events = _context.Events.Include(s => s.CreatedBy).AsNoTracking();

        //    //apply sorts and filters prior to getting total count for generating headers
        //    //this is so that headers don't return incorrect values due to filters applied
        //    //note that filters are applied first prior to sorts with Sieve.Apply
        //    events = _sieveProcessor.Apply(sieveModel, events, applyPagination: false);
        //    _paginationGenerator.GenerateHeaders(events.Count(), pageSize, pageNumber, sieveModel.Sorts, sieveModel.Filters);

        //    //apply the pagination and don't re-apply filtering and sorting as they have already been completed
        //    var eventsToReturn = _sieveProcessor.Apply(sieveModel, events, applyFiltering: false, applySorting: false);
        //    return await eventsToReturn.ToListAsync();
        //}

        public IQueryable<Event> GetEvents()
        {
            return _context.Events.Include(s => s.CreatedBy)
                           .OrderByDescending(s => s.Created)
                           .AsNoTracking();
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Events.Include(x => x.CreatedBy)
                                  .Where(x => ids.Contains(x.Id))
                                  .OrderByDescending(x => x.ScheduledTime)
                                  .ToListAsync();
        }

        public Task<Event> GetCurrentDueEventAsync()
        {
            return _context.Events.Include(s => s.CreatedBy)
                           .Where(x => x.ScheduledTime >= DateTime.Now)
                           .OrderByDescending(x => x.ScheduledTime)
                           .FirstOrDefaultAsync();
        }

        public void AddEvent(Event eventToAdd)
        {
            if (eventToAdd == null)
            {
                throw new ArgumentNullException(nameof(eventToAdd));
            }

            _context.Events.Add(eventToAdd);
        }

        public void DeleteEvent(Event eventToDelete)
        {
            if (eventToDelete == null)
            {
                throw new ArgumentNullException(nameof(eventToDelete));
            }

            _context.Events.Remove(eventToDelete);
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
            return (await _context.SaveChangesAsync() >= 0);
        }

        public async Task<bool> EventExistsAsync(Guid id)
        {
            return await _context.Events.AnyAsync(x => x.Id == id);
        }

        public void UpdateEvent(Event eventToUpdate)
        {
            //nothing required
            //Mapping EventUpdateDto to Event automatically makes the EF context track the object
            //The Event will start being tracked and can be updated
        }
    }
}
