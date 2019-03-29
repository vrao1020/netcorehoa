using Microsoft.AspNetCore.Http;

namespace HoaWebApplication.Models.EmailModels
{
    public class EmailDto
    {
        public string Message { get; set; }
        public string Name { get; set; }
        public string EmailTo { get; set; } = "";//temp string for passing model validation. It will be replaced with correct emails
        public string EmailFrom { get; set; } = "hoa@hoa.com";
        public string Subject { get; set; }
        public IFormFile Attachment { get; set; }
    }
}
