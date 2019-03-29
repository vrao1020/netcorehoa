using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HoaWebApplication.Models.FileModels
{
    public class RequiredFileDto
    {
        /// <summary>
        /// File that user uploads. This will only be used to bind the file that user uploads. In the database, only the reference to 
        /// the file name is stored as it will be stored on Azure
        /// </summary>
        [Display(Name = "File To Upload")]
        public IFormFile FileToUpload { get; set; }
    }
}
