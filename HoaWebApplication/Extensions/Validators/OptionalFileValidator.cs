using FluentValidation;
using HoaWebApplication.Extensions.CustomValidators;
using HoaWebApplication.Models.FileModels;
using HoaWebApplication.Services.File;

namespace HoaWebApplication.Extensions.Validators
{
    public class OptionalFileValidator : AbstractValidator<OptionalFileDto>
    {
        public OptionalFileValidator(IFileValidate fileValidate)
        {
            RuleFor(file => file.FileToUpload).SetValidator(new CustomFileValidator(fileValidate, false, true));
        }
    }
}
