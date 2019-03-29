using HoaEntities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaInfrastructure.Repositories
{
    public interface IBoardMeetingRepository
    {
        //Task<IEnumerable<BoardMeeting>> GetBoardMeetingsAsync(SieveModel sieveModel);
        IQueryable<BoardMeeting> GetBoardMeetings();
        Task<BoardMeeting> GetBoardMeetingAsync(Guid boardMeetingId);
        Task<BoardMeeting> GetCurrentDueBoardMeetingAsync();
        void AddBoardMeeting(BoardMeeting boardMeetingToAdd); //This not asynchronous because the object is added to the db context for tracking but its not saved yet.
                                         //So there is no need to make this async as saving the object will take care of the async operation
        void DeleteBoardMeeting(BoardMeeting boardMeetingToDelete);
        Task<bool> SaveChangesAsync();
        Task<bool> BoardMeetingExistsAsync(Guid id);
        void UpdateBoardMeeting(BoardMeeting boardMeetingToUpdate);
        Task<IEnumerable<BoardMeeting>> GetBoardMeetingsAsync(IEnumerable<Guid> ids);
    }
}
