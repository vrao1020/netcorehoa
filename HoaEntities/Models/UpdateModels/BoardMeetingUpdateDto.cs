using HoaEntities.Models.ManipulationModels;

namespace HoaEntities.Models.UpdateModels
{
    public class BoardMeetingUpdateDto: BoardMeetingManipulationDto
    {
        /// <summary>
        /// Notes for the Meeting
        /// This item can be optionally provided as its not required
        /// </summary>
        public MeetingMinuteUpdateDto MeetingNotes { get; set; }
    }
}
