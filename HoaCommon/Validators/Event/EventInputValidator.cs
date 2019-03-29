using HoaEntities.Models.InputModels;

namespace HoaCommon.Validators
{
    public class EventInputValidator : EventBaseValidator<EventInputDto>
    {
        public EventInputValidator()
        {
            //all property validators are inherited from EventManipulationValidator
        }
    }
}
