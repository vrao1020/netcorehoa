using System;

namespace HoaEntities.Models.OutputModels
{
    public class MeetingMinuteViewDto
    {
        public Guid Id { get; set; }
        public Guid BoardMeetingId { get; set; }
        public string FileName { get; set; }
        public DateTime Created { get; set; }
        public int DaysOld { get; set; } //Number of days old the meeting minute is
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; } //Email of the user who created the meeting minute
        public string OwnerSocialId { get; set; } //Social Id of the owner who owns the Meeting Minute
    }
}
