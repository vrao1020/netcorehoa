using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HoaWebApplication.Models.EmailModels;
using HoaWebApplication.Services.Email;
using HoaWebApplication.Services.File;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoaWebApplication.Pages.Contact
{
    [Authorize]
    public class ContactBoardModel : PageModel
    {
        private IFileValidate _fileValidate;
        private ISendEmail _sendEmail;

        [BindProperty]
        public EmailDto EmailProperties { get; set; }

        public ContactBoardModel(IFileValidate fileValidate, ISendEmail sendEmail)
        {
            _fileValidate = fileValidate;
            _sendEmail = sendEmail;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }

        public IActionResult OnGetFetchSendBoardEmailPartial()
        {
            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            //call the partial page and pass in the current instance of the MeetingMinuteModel class
            //this keyword refers to the current instance of this class
            //this is required as we need to check the values of some 
            return Partial("_SendBoardEmailPartial", this);
        }

        public async Task<IActionResult> OnPostBoardEmailAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_SendBoardEmailPartial", this);
            }

            if (EmailProperties.Attachment != null)
            {
                //ValidateFile returns a tuple that is deconstructed into separate variables             
                var (FileType, FileExtension) = _fileValidate.GetFileTypeExtension(EmailProperties.Attachment);

                var result = await _sendEmail.SendBoardEmailMessageAsync(EmailProperties, true, FileType, cancellationToken);

                if (result != HttpStatusCode.Accepted)
                {
                    //failed to send an email due to the SendGrid service being down or rejecting the request
                    TempData["Error"] = "An error occured while processing your request. Please try again later.";
                    return Partial("_SendBoardEmailPartial", this);
                }
            }
            else
            {
                //file was not provided so no need to attach a file
                var result = await _sendEmail.SendBoardEmailMessageAsync(EmailProperties, false, null, cancellationToken);

                if (result != HttpStatusCode.Accepted)
                {
                    //failed to send an email due to the SendGrid service being down or rejecting the request
                    TempData["Error"] = "An error occured while processing your request. Please try again later.";
                    return Partial("_SendBoardEmailPartial", this);
                }
            }

            TempData["EmailSent"] = "Your email was sent successfully. We will contact you as soon as possible";
            return Partial("_SendBoardEmailPartial", this);
        }
    }
}