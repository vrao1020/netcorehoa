using FluentValidation;
using HoaWebApplication.Extensions.CustomValidators;
using HoaWebApplication.Models.EmailModels;
using HoaWebApplication.Services.File;

namespace HoaWebApplication.Extensions.Validators
{
    public class EmailValidator : AbstractValidator<EmailDto>
    {
        public EmailValidator(IFileValidate fileValidate)
        {
            RuleFor(email => email.Message).NotNull().WithMessage("Message cannot be null")
                .Length(5, 1000).WithMessage("Message has to be between 5 and 1000 characters");

            RuleFor(email => email.EmailFrom).NotNull().WithMessage("From email cannot be null")
            .EmailAddress().WithMessage("From email has to be an email address");

            //might need to replace this with a custom validator
            RuleFor(email => email.EmailTo).NotNull().WithMessage("To email cannot be null");

            RuleFor(email => email.Subject).NotNull().WithMessage("Subject cannot be null");

            RuleFor(email => email.Name).NotNull().WithMessage("Name cannot be null");

            RuleFor(email => email.Attachment).SetValidator(new CustomFileValidator(fileValidate, false, true));
        }

    }
}
