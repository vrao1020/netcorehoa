using HoaEntities.Models.InputModels;

namespace HoaCommon.Validators
{
    public class PostInputValidator: PostBaseValidator<PostInputDto>
    {
        public PostInputValidator()
        {
            //all property validators are inherited from PostManipulationValidator
        }
    }
}
