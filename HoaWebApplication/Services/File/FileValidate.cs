using System.Collections.Generic;
using System.IO;
using System.Net;
using HoaWebApplication.Services.Azure;
using Microsoft.AspNetCore.Http;

namespace HoaWebApplication.Services.File
{
    public class FileValidate : IFileValidate
    {
        private List<string> allowedFileExtensions = new List<string> { "text/plain", "application/pdf", "application/ms-word" };
        private List<string> allowedImageExtensions = new List<string> { "image/jpeg", "image/bmp", "image/png" };
        private IAzureBlob _azureBlob;

        public FileValidate(IAzureBlob azureBlob)
        {
            _azureBlob = azureBlob;
        }

        public (string FileType, string FileExtension) GetFileTypeExtension(IFormFile fileToValidate)
        {
            if (fileToValidate.ContentType.ToLower() == "text/plain")
            {
                return ("text/plain", ".txt");
            }
            else if (fileToValidate.ContentType.ToLower() == "application/pdf")
            {
                return ("application/pdf", ".pdf");
            }
            else
            {
                return ("application/ms-word", ".docx");
            }
        }

        public (string FileType, string FileExtension) GetImageTypeExtension(IFormFile fileToValidate)
        {
            if (fileToValidate.ContentType.ToLower() == "image/jpeg")
            {
                return ("image/jpeg", ".jpg");
            }
            else if (fileToValidate.ContentType.ToLower() == "image/png")
            {
                return ("image/png", ".png");
            }
            else
            {
                return ("image/bmp", ".bmp");
            }
        }

        public (bool Valid, string ErrorMessage) ValidateFile(IFormFile fileToValidate, bool isImage)
        {
            // Use Path.GetFileName to obtain the file name, which will
            // strip any path information passed as part of the
            // FileName property. HtmlEncode the result in case it must 
            // be returned in an error message.
            var fileName = WebUtility.HtmlEncode(Path.GetFileName(fileToValidate.FileName));

            //if image type, verify file passes basic checks for type and size
            if (isImage)
            {
                if (!allowedImageExtensions.Contains(fileToValidate.ContentType.ToLower()))
                {
                    return (false, $"The image ({fileName}) must be a jpeg, png, or bmp.");
                }
                else if (fileToValidate.Length == 0)
                {
                    return (false, $"The image ({fileName}) is empty.");
                }
                else if (fileToValidate.Length > 2097152)
                {
                    return (false, $"The image ({fileName}) exceeds 2 MB.");
                }
            }
            else //is not an image so do regular file validation for file type and file size
            {
                if (!allowedFileExtensions.Contains(fileToValidate.ContentType.ToLower()))
                {
                    return (false, $"The file ({fileName}) must be a text file, pdf, or word document.");
                }
                else if (fileToValidate.Length == 0)
                {
                    return (false, $"The file ({fileName}) is empty.");
                }
                else if (fileToValidate.Length > 5048576)
                {
                    return (false, $"The file ({fileName}) exceeds 5 MB.");
                }
            }

            //file is validated so return true
            return (true, null);
        }
    }
}
