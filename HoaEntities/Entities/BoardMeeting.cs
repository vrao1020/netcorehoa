using System;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Entities
{
    public class BoardMeeting
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Required, MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required, MaxLength(300)]
        public string ScheduledLocation { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public MeetingMinute MeetingNotes { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User CreatedBy { get; set; }
    }
}
