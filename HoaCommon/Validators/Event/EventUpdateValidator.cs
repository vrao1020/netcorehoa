using HoaEntities.Models.UpdateModels;

namespace HoaCommon.Validators
{
    public class EventUpdateValidator : EventBaseValidator<EventUpdateDto>
    {
        public EventUpdateValidator()
        {
            //all property validators are inherited from EventManipulationValidator
        }
    }
}
