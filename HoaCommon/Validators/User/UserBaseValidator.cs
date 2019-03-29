using FluentValidation;
using HoaEntities.Models.ManipulationModels;

namespace HoaCommon.Validators
{
    public class UserBaseValidator<T> : AbstractValidator<T> where T: UserManipulationDto
    {
        public UserBaseValidator()
        {
            RuleFor(user => user.FirstName).NotNull().WithMessage("First Name cannot be null")
                .Length(1, 50);

            RuleFor(user => user.LastName).NotNull().WithMessage("Last Name cannot be null")
                .Length(1, 50);

            RuleFor(user => user.Reminder).NotNull().WithMessage("Reminder cannot be null");

            RuleFor(user => user.Email).NotNull().WithMessage("Email cannot be null")
                .EmailAddress();//.WithMessage("Email must be in a valid format");
        }
    }
}
