using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string SocialId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public bool Reminder { get; set; } = false;

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool Active { get; set; } = true;

        public ICollection<Post> Posts { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<MeetingMinute> MeetingMinutes { get; set; }
        public ICollection<BoardMeeting> BoardMeetings { get; set; }
        public ICollection<Comment> Comments { get; set; }

        [Required]
        public DateTime? Joined { get; set; }

    }
}
