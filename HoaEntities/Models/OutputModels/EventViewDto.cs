using System;

namespace HoaEntities.Models.OutputModels
{
    public class EventViewDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ImageName { get; set; }
        public string ThumbnailName { get; set; }
        public DateTime Created { get; set; }
        public int DaysOld { get; set; } //Number of days old the event is
        public DateTime ScheduledTime { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; } //Email of the user who created the event
        public string OwnerSocialId { get; set; } //Social Id of the owner who owns the event
    }
}
