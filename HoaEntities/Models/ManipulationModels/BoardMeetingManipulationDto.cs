using System;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Models.ManipulationModels
{
    public class BoardMeetingManipulationDto
    {
        /// <summary>
        /// Name of the Board Meeting
        /// </summary>
        [Display(Name = "Name")]
        public string Title { get; set; }
        /// <summary>
        /// Description of the Meeting
        /// </summary>
        [Display(Name = "Agenda")]
        public string Description { get; set; }
        /// <summary>
        /// Scheduled time for the Meeting
        /// </summary>
        [Display(Name = "Date/Time")]
        public DateTime? ScheduledTime { get; set; }
        /// <summary>
        /// Scheduled location for the Meeting
        /// </summary>
        [Display(Name = "Location")]
        public string ScheduledLocation { get; set; }
    }
}
