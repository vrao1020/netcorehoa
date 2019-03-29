using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HoaWebApplication.Models.EmailModels;
using HoaWebApplication.Services.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoaWebApplication.Pages.Contact
{
    public class ContactAdminModel : PageModel
    {
        private ISendEmail _sendEmail;

        [BindProperty]
        public EmailDto EmailProperties { get; set; }

        public ContactAdminModel(ISendEmail sendEmail)
        {
            _sendEmail = sendEmail;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }

        public IActionResult OnGetFetchSendAdminEmailPartial()
        {
            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            //call the partial page and pass in the current instance of the MeetingMinuteModel class
            //this keyword refers to the current instance of this class
            //this is required as we need to check the values of some 
            return Partial("_SendAdminEmailPartial", this);
        }

        public async Task<IActionResult> OnPostAdminEmailAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_SendAdminEmailPartial", this);
            }

            //send the email
            var result = await _sendEmail.SendAdminEmailMessageAsync(EmailProperties, cancellationToken);

            if (result != HttpStatusCode.Accepted)
            {
                //failed to send an email due to the SendGrid service being down or rejecting the request
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_SendAdminEmailPartial", this);
            }

            TempData["EmailSent"] = "Your email was sent successfully. We will contact you as soon as possible";
            return Partial("_SendAdminEmailPartial", this);
        }
    }
}