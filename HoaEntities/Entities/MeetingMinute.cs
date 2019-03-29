using System;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Entities
{
    public class MeetingMinute
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string FileName { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public Guid BoardMeetingId { get; set; }

        [Required]
        public BoardMeeting Meeting { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User CreatedBy { get; set; }
    }
}
