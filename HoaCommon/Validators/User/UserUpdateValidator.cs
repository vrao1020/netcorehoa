using HoaEntities.Models.UpdateModels;

namespace HoaCommon.Validators
{
    public class UserUpdateValidator : UserBaseValidator<UserUpdateDto>
    {
        public UserUpdateValidator()
        {
            //all property validators are inherited from UserManipulationValidator
        }
    }
}
