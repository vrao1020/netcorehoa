using System;

namespace HoaEntities.Models.OutputModels
{
    public class PostViewDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public int DaysOld { get; set; } //Number of days old the post is
        public int NumberOfComments { get; set; } //Number of comments on the post
        public bool Important { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; } //Email of the user who created the post
        public string OwnerSocialId { get; set; } //Social Id of the owner who owns the Meeting Minute
    }
}
