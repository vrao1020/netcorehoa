using HoaEntities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaInfrastructure.Repositories
{
    public interface IMeetingMinuteRepository
    {
        //Task<IEnumerable<MeetingMinute>> GetMeetingMinutesAsync(SieveModel sieveModel);
        IQueryable<MeetingMinute> GetMeetingMinutes();
        Task<MeetingMinute> GetMeetingMinuteAsync(Guid boardMeetingId, Guid meetingMinuteId);
        Task AddMeetingMinute(Guid boardMeetingId, MeetingMinute meetingMinuteToAdd); //This not asynchronous because the object is added to the db context for tracking but its not saved yet.
                                                 //So there is no need to make this async as saving the object will take care of the async operation
        void DeleteMeetingMinute(MeetingMinute meetingMinuteToDelete);
        Task<bool> SaveChangesAsync();
        Task<bool> MeetingMinuteExistsAsync(Guid boardMeetingId, Guid id);
        void UpdateMeetingMinute(Guid boardMeetingId, MeetingMinute meetingMinuteToUpdate);
        Task<IEnumerable<MeetingMinute>> GetMeetingMinutesAsync(Guid boardMeetingId, IEnumerable<Guid> ids);
    }
}
