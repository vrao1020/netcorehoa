using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaInfrastructure.Context;
using HoaEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoaInfrastructure.Repositories
{
    public class BoardMeetingRepository : IBoardMeetingRepository
    {
        private HoaDbContext _context;

        public BoardMeetingRepository(HoaDbContext hoaDbContextbContext)
        {
            _context = hoaDbContextbContext ?? throw new ArgumentNullException(nameof(hoaDbContextbContext));
        }

        public void AddBoardMeeting(BoardMeeting boardMeetingToAdd)
        {
            if (boardMeetingToAdd == null)
            {
                throw new ArgumentNullException(nameof(boardMeetingToAdd));
            }

            _context.BoardMeetings.Add(boardMeetingToAdd);
        }

        public async Task<bool> BoardMeetingExistsAsync(Guid id)
        {
            return await _context.BoardMeetings.AnyAsync(x => x.Id == id);
        }

        public void DeleteBoardMeeting(BoardMeeting boardMeetingToDelete)
        {
            if (boardMeetingToDelete == null)
            {
                throw new ArgumentNullException(nameof(boardMeetingToDelete));
            }

            _context.BoardMeetings.Remove(boardMeetingToDelete);
        }

        public async Task<BoardMeeting> GetBoardMeetingAsync(Guid boardMeetingId)
        {
            var boardMeetingToReturn = await _context.BoardMeetings
                                                     .Include(s => s.CreatedBy)
                                                     .Include(s => s.MeetingNotes)
                                                     .FirstOrDefaultAsync(x => x.Id == boardMeetingId);
            return boardMeetingToReturn;
        }

        public IQueryable<BoardMeeting> GetBoardMeetings()
        {
            return _context.BoardMeetings.Include(s => s.CreatedBy)
                                         .Include(s => s.MeetingNotes)
                                         .OrderByDescending(s => s.Created)
                                         .AsNoTracking();
        }

        //public async Task<IEnumerable<BoardMeeting>> GetBoardMeetingsAsync(SieveModel sieveModel)
        //{
        //    //get max and default page values
        //    var maxPageSize = Int32.Parse(_configuration["Sieve:MaxPageSize"]);
        //    var defaultPageSize = Int32.Parse(_configuration["Sieve:DefaultPageSize"]);

        //    //set page size and number and check for null values as the library does not support default values
        //    var pageSize = sieveModel.PageSize ?? defaultPageSize;
        //    var pageNumber = sieveModel.Page ?? 1;
        //    pageSize = pageSize > maxPageSize ? defaultPageSize : pageSize;

        //    //fetch the meetings from the repo
        //    var boardMeetings = _context.BoardMeetings.Include(s => s.CreatedBy)
        //                                .Include(s => s.MeetingNotes)
        //                                .AsNoTracking();

        //    //apply sorts and filters prior to getting total count for generating headers
        //    //this is so that headers don't return incorrect values due to filters applied
        //    //note that filters are applied first prior to sorts with Sieve.Apply
        //    boardMeetings = _sieveProcessor.Apply(sieveModel, boardMeetings, applyPagination: false);
        //    _paginationGenerator.GenerateHeaders(boardMeetings.Count(), pageSize, pageNumber, sieveModel.Sorts, sieveModel.Filters);

        //    //apply the pagination and don't re-apply filtering and sorting as they have already been completed
        //    var boardMeetingsToReturn = _sieveProcessor.Apply(sieveModel, boardMeetings, applyFiltering: false, applySorting: false);
        //    return await boardMeetingsToReturn.ToListAsync();
        //}

        public async Task<IEnumerable<BoardMeeting>> GetBoardMeetingsAsync(IEnumerable<Guid> ids)
        {
            return await _context.BoardMeetings.Include(x => x.CreatedBy)
                                  .Include(x => x.MeetingNotes)
                                  .Where(x => ids.Contains(x.Id))
                                  .OrderByDescending(x => x.ScheduledTime)
                                  .ToListAsync();
        }

        public Task<BoardMeeting> GetCurrentDueBoardMeetingAsync()
        {
            return _context.BoardMeetings.Include(s => s.CreatedBy)
                           .Where(x => x.ScheduledTime >= DateTime.Now)
                           .OrderByDescending(x => x.ScheduledTime)
                           .FirstOrDefaultAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void UpdateBoardMeeting(BoardMeeting boardMeetingToUpdate)
        {
            //nothing required
            //Mapping BoardMeetingUpdateDto to BoardMeeting automatically makes the EF context track the object
            //The Event will start being tracked and can be updated
        }
    }
}
