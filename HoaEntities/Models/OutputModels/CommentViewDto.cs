using System;

namespace HoaEntities.Models.OutputModels
{
    public class CommentViewDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public Guid? ParentCommentId { get; set; } //id of the comment that is the parent of another comment
        public int DaysOld { get; set; } //Number of days old the comment is
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; } //Email of the user who created the post
        public Guid PostId { get; set; }
        public string OwnerSocialId { get; set; } //Social Id of the owner who owns the Meeting Minute
    }
}
