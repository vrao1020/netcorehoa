using System.ComponentModel.DataAnnotations;

namespace HoaEntities.Models.ManipulationModels
{
    public class MeetingMinuteManipulationDto
    {
        /// <summary>
        /// Name of the file on Azure
        /// </summary>
        [Display(Name = "File To Upload")]
        public string FileName { get; set; }
    }
}
