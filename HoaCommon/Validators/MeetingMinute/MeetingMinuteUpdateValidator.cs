using HoaEntities.Models.UpdateModels;

namespace HoaCommon.Validators
{
    public class MeetingMinuteUpdateValidator: MeetingMinuteBaseValidator<MeetingMinuteUpdateDto>
    {
        public MeetingMinuteUpdateValidator()
        {
            //all property validators are inherited from EventManipulationValidator
        }
    }
}
