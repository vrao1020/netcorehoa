using System;

namespace HoaEntities.Models.OutputModels
{
    public class BoardMeetingViewDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public int DaysOld { get; set; } //Number of days old the meeting is
        public DateTime ScheduledTime { get; set; }
        public string ScheduledLocation { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; } //Email of the user who created the meeting
        public string OwnerSocialId { get; set; } //Social Id of the owner who owns the Board Meeting
        public Guid? MeetingMinuteId { get; set; }
        public string FileName { get; set; }
    }
}
