using HoaEntities.Models.ManipulationModels;
using System;

namespace HoaEntities.Models.InputModels
{
    public class CommentInputDto : CommentManipulationDto
    {
        /// <summary>
        /// Id of the parent comment - if null, comment does not have a parent
        /// </summary>
        public Guid? ParentCommentId { get; set; }
    }
}
