using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Entities
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public bool Important { get; set; }

        public ICollection<Comment> Comments { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User CreatedBy { get; set; }

    }
}
