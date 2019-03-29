using FluentValidation.Validators;
using HoaWebApplication.Services.File;
using Microsoft.AspNetCore.Http;

namespace HoaWebApplication.Extensions.CustomValidators
{
    public class CustomFileValidator : PropertyValidator
    {
        private IFileValidate _fileValidate;
        private bool _isImage;
        private bool _isBoardEmail;

        /// <summary>
        /// Custom validator for validating images and files.If isImage is passed as true, the file is not required. 
        /// If isBoardEmail is passed as true, the file is not required. 
        /// All other instances will cause the file to be required / not null
        /// </summary>
        /// <param name="fileValidate"></param>
        /// <param name="isImage"></param>
        /// <param name="isBoardEmail"></param>
        public CustomFileValidator(IFileValidate fileValidate, bool isImage, bool isBoardEmail) : base("{ErrorMessage}")
        {
            _fileValidate = fileValidate;
            _isImage = isImage;
            _isBoardEmail = isBoardEmail;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var fileToValidate = context.PropertyValue as IFormFile;

            //if the user did not upload an image, return true as we don't need to validate anything
            if (_isImage && fileToValidate == null)
            {
                return true;
            }
            //if the user did not upload a file for board email, return true as we don't need to validate anything
            else if (_isBoardEmail && fileToValidate == null)
            {
                return true;
            }
            //otherwise if the file is null (in case of meetingminutes) - user must upload a file so return false
            else if (fileToValidate == null)
            {
                return false;
            }

            var (Valid, ErrorMessage) = _fileValidate.ValidateFile(fileToValidate, _isImage);

            if (!Valid)
            {
                context.MessageFormatter.AppendArgument("ErrorMessage", ErrorMessage);
                return false;
            }

            return true;
        }
    }

}
