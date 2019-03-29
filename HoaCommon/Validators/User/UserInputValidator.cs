using HoaEntities.Models.InputModels;

namespace HoaCommon.Validators
{
    public class UserInputValidator : UserBaseValidator<UserInputDto>
    {
        public UserInputValidator()
        {
            //all property validators are inherited from UserManipulationValidator
        }
    }
}
