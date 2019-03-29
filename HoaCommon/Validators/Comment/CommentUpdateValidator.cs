using HoaEntities.Models.UpdateModels;

namespace HoaCommon.Validators
{
    public class CommentUpdateValidator : CommentBaseValidator<CommentUpdateDto>
    {
        public CommentUpdateValidator()
        {
            //all property validators are inherited from CommentManipulationValidator
        }
    }
}
