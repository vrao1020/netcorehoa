using HoaEntities.Models.UpdateModels;

namespace HoaCommon.Validators
{
    public class BoardMeetingUpdateValidator : BoardMeetingBaseValidator<BoardMeetingUpdateDto>
    {
        public BoardMeetingUpdateValidator()
        {
            //all property validators are inherited from BoardMeetingManipulationValidator
        }
    }
}
