using HoaEntities.Models.InputModels;

namespace HoaCommon.Validators
{
    public class BoardMeetingInputValidator: BoardMeetingBaseValidator<BoardMeetingInputDto>
    {
        public BoardMeetingInputValidator()
        {
            //all property validators are inherited from BoardMeetingManipulationValidator
        }
    }
}
