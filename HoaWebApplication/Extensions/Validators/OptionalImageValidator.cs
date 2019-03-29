using FluentValidation;
using HoaWebApplication.Extensions.CustomValidators;
using HoaWebApplication.Models.ImageModels;
using HoaWebApplication.Services.File;

namespace HoaWebApplication.Extensions.Validators
{
    public class OptionalImageValidator : AbstractValidator<OptionalImageDto>
    {
        public OptionalImageValidator(IFileValidate fileValidate)
        {
            RuleFor(file => file.FileToUpload).SetValidator(new CustomFileValidator(fileValidate, true, false));
        }
    }
}
