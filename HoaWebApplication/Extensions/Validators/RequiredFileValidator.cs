using FluentValidation;
using HoaWebApplication.Extensions.CustomValidators;
using HoaWebApplication.Models.FileModels;
using HoaWebApplication.Services.File;

namespace HoaWebApplication.Extensions.Validators
{
    public class RequiredFileValidator : AbstractValidator<RequiredFileDto>
    {
        public RequiredFileValidator(IFileValidate fileValidate)
        {
            RuleFor(file => file.FileToUpload).NotNull().SetValidator(new CustomFileValidator(fileValidate, false, false));
        }
    }
}
