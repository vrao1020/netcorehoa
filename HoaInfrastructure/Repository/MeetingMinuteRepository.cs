using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaInfrastructure.Context;
using HoaEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoaInfrastructure.Repositories
{
    public class MeetingMinuteRepository : IMeetingMinuteRepository
    {
        private HoaDbContext _hoaDbContext;

        public MeetingMinuteRepository(HoaDbContext hoaDbContext)
        {
            _hoaDbContext = hoaDbContext;
        }

        //public async Task<IEnumerable<MeetingMinute>> GetMeetingMinutesAsync(SieveModel sieveModel)
        //{
        //    //get max and default page values
        //    var maxPageSize = Int32.Parse(_configuration["Sieve:MaxPageSize"]);
        //    var defaultPageSize = Int32.Parse(_configuration["Sieve:DefaultPageSize"]);

        //    //set page size and number and check for null values as the library does not support default values
        //    var pageSize = sieveModel.PageSize ?? defaultPageSize;
        //    var pageNumber = sieveModel.Page ?? 1;
        //    pageSize = pageSize > maxPageSize ? defaultPageSize : pageSize;

        //    //fetch the events from the repo
        //    var meetingsMinutes = _hoaDbContext.MeetingMinutes.Include(minute => minute.CreatedBy)
        //                                                      .AsNoTracking();

        //    //apply sorts and filters prior to getting total count for generating headers
        //    //this is so that headers don't return incorrect values due to filters applied
        //    //note that filters are applied first prior to sorts with Sieve.Apply
        //    meetingsMinutes = _sieveProcessor.Apply(sieveModel, meetingsMinutes, applyPagination: false);
        //    _paginationGenerator.GenerateHeaders(meetingsMinutes.Count(), pageSize, pageNumber, sieveModel.Sorts, sieveModel.Filters);

        //    //apply the pagination and don't re-apply filtering and sorting as they have already been completed
        //    var meetingMinutesToReturn = _sieveProcessor.Apply(sieveModel, meetingsMinutes, applyFiltering: false, applySorting: false);

        //    return await meetingMinutesToReturn.ToListAsync();
        //}

        public IQueryable<MeetingMinute> GetMeetingMinutes()
        {
            return _hoaDbContext.MeetingMinutes.Include(minute => minute.CreatedBy)
                                               .OrderByDescending(minute => minute.Created)
                                               .AsNoTracking();
        }

        public Task<MeetingMinute> GetMeetingMinuteAsync(Guid boardMeetingId, Guid meetingMinuteId)
        {
            return _hoaDbContext.MeetingMinutes.Include(minute => minute.CreatedBy)
                                               .Where(minute => minute.BoardMeetingId == boardMeetingId)
                                               .FirstOrDefaultAsync(minute => minute.Id == meetingMinuteId);
        }

        public async Task<IEnumerable<MeetingMinute>> GetMeetingMinutesAsync(Guid boardMeetingId, IEnumerable<Guid> ids)
        {
            return await _hoaDbContext.MeetingMinutes.Include(minute => minute.CreatedBy)
                                               .Where(minute => ids.Contains(minute.Id) && minute.BoardMeetingId == boardMeetingId)
                                               .OrderByDescending(minute => minute.Created)
                                               .ToListAsync();
        }

        public async Task AddMeetingMinute(Guid boardMeetingId, MeetingMinute meetingMinuteToAdd)
        {
            if (meetingMinuteToAdd == null)
            {
                throw new ArgumentNullException(nameof(meetingMinuteToAdd));
            }

            //we are adding a related entity here
            //note that no null check if done here because the controller will take care of checking
            //if the meeting exists or not. The repository is purely for adding/updating/deleting
            var boardMeeting = await _hoaDbContext.BoardMeetings.FirstOrDefaultAsync(meeting => meeting.Id == boardMeetingId);

            //add the meeting minute to the board meeting
            boardMeeting.MeetingNotes = meetingMinuteToAdd;
        }

        public void DeleteMeetingMinute(MeetingMinute meetingMinuteToDelete)
        {
            if (meetingMinuteToDelete == null)
            {
                throw new ArgumentNullException(nameof(meetingMinuteToDelete));
            }

            _hoaDbContext.MeetingMinutes.Remove(meetingMinuteToDelete);
        }

        public async Task<bool> MeetingMinuteExistsAsync(Guid boardMeetingId, Guid id)
        {
            return await _hoaDbContext.MeetingMinutes.AnyAsync(minute => minute.Id == id && minute.BoardMeetingId == boardMeetingId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
            return (await _hoaDbContext.SaveChangesAsync()) >= 0;
        }

        public void UpdateMeetingMinute(Guid boardMeetingId, MeetingMinute meetingMinuteToUpdate)
        {
            //nothing required
            //Mapping MeetingMinuteUpdateDto to MeetingMinute automatically makes the EF context track the object
            //The MeetingMinute will start being tracked and can be updated
        }
    }
}
