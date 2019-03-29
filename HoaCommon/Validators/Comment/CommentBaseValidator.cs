using FluentValidation;
using HoaEntities.Models.ManipulationModels;

namespace HoaCommon.Validators
{
    public class CommentBaseValidator<T> : AbstractValidator<T> where T : CommentManipulationDto
    {
        public CommentBaseValidator()
        {
            RuleFor(manipulationDto => manipulationDto.Message).NotNull().WithMessage("Message cannot be null")
              .Length(1, 50);
        }
    }
}
