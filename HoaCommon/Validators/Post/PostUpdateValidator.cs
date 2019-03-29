using HoaEntities.Models.UpdateModels;

namespace HoaCommon.Validators
{
    public class PostUpdateValidator: PostBaseValidator<PostUpdateDto>
    {
        public PostUpdateValidator()
        {
            //all property validators are inherited from PostManipulationValidator
        }
    }
}
