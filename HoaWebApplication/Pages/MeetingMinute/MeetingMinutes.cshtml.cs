using System;
using System.Threading;
using HoaWebApplication.Services.Azure;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HoaWebApplication.Pages.MeetingMinute
{
    public class MeetingMinutesModel : PageModel
    {
        private IAzureSASTokenUrl _azureSASTokenUrl;
        private ILogger<MeetingMinutesModel> _logger;
        private const string AzureBlobFolder = "meetingminutes"; //azure blob storage folder where files will be stored/fetched

        //used for GET action using the Download handler to download the file
        [BindProperty(SupportsGet = true)]
        public string FileName { get; set; }

        public MeetingMinutesModel(IAzureSASTokenUrl azureSASTokenUrl, ILogger<MeetingMinutesModel> logger)
        {
            _azureSASTokenUrl = azureSASTokenUrl;
            _logger = logger;
        }

        public IActionResult OnGet(CancellationToken cancellationToken)
        {
            return RedirectToPage("/BoardMeeting/BoardMeetings");
        }

        //this action is used for downloading files
        public IActionResult OnGetDownload()
        {
            if (FileName == null)
            {
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Page();
            }

            try
            {
                //get the url for the file from Azure 
                var urlToDownload = _azureSASTokenUrl.GetDownloadUrlWithSAS(FileName, AzureBlobFolder);
                return Redirect(urlToDownload);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Page();
            }
        }
    }
}