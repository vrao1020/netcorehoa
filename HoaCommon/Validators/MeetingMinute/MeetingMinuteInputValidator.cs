using HoaEntities.Models.InputModels;

namespace HoaCommon.Validators
{
    public class MeetingMinuteInputValidator: MeetingMinuteBaseValidator<MeetingMinuteInputDto>
    {
        public MeetingMinuteInputValidator()
        {
            //all property validators are inherited from EventManipulationValidator
        }
    }
}
