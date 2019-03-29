using Microsoft.AspNetCore.Http;

namespace HoaWebApplication.Services.File
{
    public interface IFileValidate
    {
        /// <summary>
        /// Validates the file and returns an error message when not valid and a null message when valid
        /// Optionally takes a boolean parameter that will ignore that a blob file already exists (for updating purposes)
        /// Parameter  blobStorageFolder is required to verify which blob storage folder to check for file existance
        /// Parameter isImage is used to check image files types as they have different requirements
        /// </summary>
        /// <param name="fileToValidate"></param>
        /// <param name="isImage"></param>
        /// <returns></returns>
        (bool Valid, string ErrorMessage) ValidateFile(IFormFile fileToValidate, bool isImage);
        (string FileType, string FileExtension) GetFileTypeExtension(IFormFile fileToValidate);
        (string FileType, string FileExtension) GetImageTypeExtension(IFormFile fileToValidate);
    }
}
