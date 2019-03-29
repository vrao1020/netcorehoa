using HoaEntities.Models.ManipulationModels;

namespace HoaEntities.Models.InputModels
{
    public class BoardMeetingInputDto: BoardMeetingManipulationDto
    {
        /// <summary>
        /// Notes for the Meeting
        /// This item can be optionally provided as its not required
        /// </summary>
        public MeetingMinuteInputDto MeetingNotes { get; set; }
    }
}
