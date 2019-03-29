using System;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Entities
{
    public class Event
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        public string ImageName { get; set; }

        public string ThumbnailName { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User CreatedBy { get; set; }
    }
}
