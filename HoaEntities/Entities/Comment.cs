using System;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public Guid? ParentCommentId { get; set; }

        [Required]
        public Guid PostId { get; set; }

        [Required]
        public Post CommentForPost { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User CreatedBy { get; set; }
    }
}
