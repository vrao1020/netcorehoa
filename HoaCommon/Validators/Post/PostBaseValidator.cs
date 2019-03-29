using FluentValidation;
using HoaEntities.Models.ManipulationModels;

namespace HoaCommon.Validators
{
    public class PostBaseValidator<T> : AbstractValidator<T> where T : PostManipulationDto
    {
        public PostBaseValidator()
        {
            RuleFor(manipulationDto => manipulationDto.Important).NotNull().WithMessage("You need to specify if the post is important or not");

            RuleFor(manipulationDto => manipulationDto.Title).NotNull().WithMessage("Title cannot be null")
              .Length(1, 50);

            RuleFor(manipulationDto => manipulationDto.Message).NotNull().WithMessage("Message cannot be null")
                  .Length(1, 1000);
        }
    }
}
