using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Models.ManipulationModels
{
    public class UserManipulationDto
    {
        /// <summary>
        /// User's First Name
        /// </summary>
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        /// <summary>
        /// User's Last Name
        /// </summary>
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        /// <summary>
        /// Verifies if the User wants reminder emails for Events
        /// </summary>
        [Display(Name = "Board Meeting Reminder?")]
        public bool? Reminder { get; set; } = false;
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }
    }
}
