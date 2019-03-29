using HoaEntities.Models.InputModels;

namespace HoaCommon.Validators
{
    public class CommentInputValidator : CommentBaseValidator<CommentInputDto>
    {
        public CommentInputValidator()
        {
            //all property validators are inherited from CommentManipulationValidator
        }
    }
}
