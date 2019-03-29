using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Models.ManipulationModels
{
    public class PostManipulationDto
    {
        /// <summary>
        /// Title of the Post
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Description/Content for the Post
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Verifies if the Post is important or not
        /// </summary>
        [Display(Name = "Important?")]
        public bool? Important { get; set; }
    }
}
