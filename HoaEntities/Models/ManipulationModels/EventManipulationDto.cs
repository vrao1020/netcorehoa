using System;
using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Models.ManipulationModels
{
    public class EventManipulationDto
    {
        /// <summary>
        /// Name of the Event
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Description/Content for the Event
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Url where the uploaded image is stored
        /// </summary>
        public string ImageName { get; set; }
        /// <summary>
        /// Url where the uploaded image thumbnail is stored
        /// </summary>
        public string ThumbnailName { get; set; }
        /// <summary>
        /// Scheduled time when the event will occur
        /// </summary>
        [Display(Name = "Date/Time")]
        public DateTime? ScheduledTime { get; set; }
    }
}
