using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Extensions.Streams;
using HoaCommon.Extensions.UserClaims;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.OutputModels;
using HoaWebApplication.Models.PaginationModels;
using HoaEntities.Models.UpdateModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using HoaWebApplication.Services.Azure;
using HoaWebApplication.Models.FileModels;
using HoaWebApplication.Services.File;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace HoaWebApplication.Pages.BoardMeeting
{
    public class BoardMeetingsModel : PageModel
    {
        private string baseAPIUrl;
        private ApiClient _apiClient;
        private IMapper _mapper;
        private IAzureSASTokenUrl _azureSASTokenUrl;
        private IAzureBlob _azureBlob;
        private IFileValidate _fileValidate;
        private ILogger<BoardMeetingsModel> _logger;
        private const string AzureBlobFolder = "meetingminutes"; //azure blob storage folder where files will be stored/fetched

        //Pagination parameters deserialized from API X-Pagination header. This will help paginate the data returned
        public XPaginationHeaderDto XPaginationDto;
        public IEnumerable<BoardMeetingViewDto> boardMeetings;
        public BoardMeetingViewDto boardMeeting;
        public bool EditMeeting = false;
        public bool UserIsAdmin = false;

        //this property is needed because razor pages don't allow a different model to be passed to a partial view
        //other than the base PageModel (i.e. you MUST pass in BoardMeetingsModel as the model). Otherwise it will throw an error
        //this property is set in the Create/Edit methods so that the partial view can access it
        public BoardMeetingManipulationDto MeetingToCreateEdit { get; set; }
        public RequiredFileDto FileUploaded { get; set; }

        //page number, page size, filters, and sorts that will be used for pagination
        [BindProperty(SupportsGet = true)]
        public PageParametersDto PageParameters { get; set; }

        //used for binding to the id the specific BoardMeeting that will be edited
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        //used for GET action using the Download handler to download the file
        [BindProperty(SupportsGet = true)]
        public string FileName { get; set; }

        public BoardMeetingsModel(ApiClient apiClient, IMapper mapper, IAzureSASTokenUrl azureSASTokenUrl,
            IAzureBlob azureBlob, IFileValidate fileValidate, ILogger<BoardMeetingsModel> logger)
        {
            _apiClient = apiClient;
            _mapper = mapper;
            _azureSASTokenUrl = azureSASTokenUrl;
            _azureBlob = azureBlob;
            _fileValidate = fileValidate;
            _logger = logger;
            //need to initialize an empty object because by default the property is null
            //within the view, when you try and access the objects underlying properties, razor will throw an error
            //because the property has not been initialized
            MeetingToCreateEdit = new BoardMeetingManipulationDto();
            FileUploaded = new RequiredFileDto();
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            TempData["PageNum"] = PageParameters.PageNum;
            TempData["PageSize"] = PageParameters.PageSize;

            baseAPIUrl = $"api/boardmeetings?page={PageParameters.PageNum}&pagesize={PageParameters.PageSize}";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithGETOnlyAccessAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Page();
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                var header = response.Headers.GetValues("X-Pagination").FirstOrDefault();
                XPaginationDto = JsonConvert.DeserializeObject<XPaginationHeaderDto>(header);

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                boardMeetings = streamContent.ReadAndDeserializeFromJson<IEnumerable<BoardMeetingViewDto>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Page();
            }

            //set if the user is an administrator or not. This will allow user to delete/edit all items on the page
            UserIsAdmin = (User?.IsAdministrator() ?? false);

            return Page();
        }

        //this action is used for downloading files
        public IActionResult OnGetDownload()
        {
            if (FileName == null)
            {
                TempData["FileNull"] = "Please specify a file to download";
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

        public IActionResult OnGetFetchCreateBoardMeetingPartial()
        {
            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            //call the partial page and pass in the current instance of the MeetingMinuteModel class
            //this keyword refers to the current instance of this class
            //this is required as we need to check the values of some 
            return Partial("_CreateEditBoardMeetingPartial", this);
        }

        public async Task<IActionResult> OnPostCreateMeetingAsync(CancellationToken cancellationToken, BoardMeetingManipulationDto MeetingToCreateEdit)
        {
            if (!ModelState.IsValid)
            {
                //set the pagemodel property to be the values submitted by the user
                //not doing this will cause all model state errors to be lost
                this.MeetingToCreateEdit = MeetingToCreateEdit;
                return Partial("_CreateEditBoardMeetingPartial", this);
            }

            var baseAPIUrl = $"api/boardmeetings";

            try
            {
                var meetingToCreate = _mapper.Map<BoardMeetingInputDto>(MeetingToCreateEdit);

                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PostAsJsonAsync<BoardMeetingInputDto>(baseAPIUrl,
                    meetingToCreate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditBoardMeetingPartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_CreateEditBoardMeetingPartial", this);
            }

            //if successful, return the partial view which will have the IsValid value set to true by default
            //this is because ajax is used to post the form rather than a submit button 
            return Partial("_CreateEditBoardMeetingPartial", this);
        }

        public async Task<IActionResult> OnGetFetchEditBoardMeetingPartialAsync(CancellationToken cancellationToken)
        {
            //set this value to true as the partial page will include specific 
            //elements in the modal based on this value
            EditMeeting = true;

            var baseAPIUrl = $"api/boardmeetings/{Id}";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditBoardMeetingPartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                boardMeeting = streamContent.ReadAndDeserializeFromJson<BoardMeetingViewDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_CreateEditBoardMeetingPartial", this);
            }

            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            return Partial("_CreateEditBoardMeetingPartial", this);
        }

        public async Task<IActionResult> OnPostEditMeetingAsync(CancellationToken cancellationToken, BoardMeetingManipulationDto MeetingToCreateEdit)
        {
            if (!ModelState.IsValid)
            {
                //set the pagemodel property to be the values submitted by the user
                //not doing this will cause all model state errors to be lost
                this.MeetingToCreateEdit = MeetingToCreateEdit;
                return Partial("_CreateEditBoardMeetingPartial", this);
            }

            var baseAPIUrl = $"api/boardmeetings/{Id}";

            try
            {
                var meetingToUpdate = _mapper.Map<BoardMeetingUpdateDto>(MeetingToCreateEdit);

                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PutAsJsonAsync<BoardMeetingUpdateDto>(baseAPIUrl,
                    meetingToUpdate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditBoardMeetingPartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_CreateEditBoardMeetingPartial", this);
            }

            return Partial("_CreateEditBoardMeetingPartial", this);
        }

        public async Task<IActionResult> OnPostDeleteMeetingAsync(CancellationToken cancellationToken)
        {
            var baseAPIUrl = $"api/boardmeetings/{Id}";

            //get the board meeting and verify if a file exists
            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditBoardMeetingPartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                boardMeeting = streamContent.ReadAndDeserializeFromJson<BoardMeetingViewDto>();

                //set the file name if its not null
                if (boardMeeting.FileName != null)
                {
                    FileName = boardMeeting.FileName;
                }

                //get the response with authorization as the API endpoint requires an authenticated user
                var responseForDelete = await (await _apiClient.WithAuthorization()).DeleteAsync(baseAPIUrl, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (responseForDelete.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Content("Error");
                }

                //ensure success status code else throw an exception
                responseForDelete.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Content("Error");
            }

            //after ensuring that the meeting is deleted, delete the file from blob storage
            if (FileName != null)
            {
                var blobToDelete = _azureBlob.GetAzureBlobFileReference(FileName, AzureBlobFolder);
                await _azureBlob.DeleteAzureBlobFileAsync(blobToDelete);
            }

            return Content("Success");
        }

        public IActionResult OnGetFetchUploadMeetingMinutePartial()
        {
            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            //call the partial page and pass in the current instance of the MeetingMinuteModel class
            //this keyword refers to the current instance of this class
            //this is required as we need to check the values of some 
            return Partial("_UploadMeetingMinutePartial", this);
        }

        public async Task<IActionResult> OnPostUploadMeetingMinuteAsync(CancellationToken cancellationToken, [FromForm] RequiredFileDto FileUploaded)
        {
            if (!ModelState.IsValid)
            {
                //need to manually add this due to a bug in the model binding process for .net core
                //see issue here https://github.com/JeremySkinner/FluentValidation/issues/1029
                ModelState.AddModelError("FileUploaded.FileToUpload",
                    ModelState.Keys.Where(k => k == "FileToUpload").Select(k => ModelState[k].Errors[0].ErrorMessage).First());

                //set the pagemodel property to be the values submitted by the user
                //not doing this will cause all model state errors to be lost
                this.FileUploaded = FileUploaded;
                return Partial("_UploadMeetingMinutePartial", this);
            }

            var baseAPIUrl = $"api/boardmeetings/{Id}";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_UploadMeetingMinutePartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                boardMeeting = streamContent.ReadAndDeserializeFromJson<BoardMeetingViewDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_UploadMeetingMinutePartial", this);
            }

            //GetFileTypeExtension returns a tuple that is deconstructed into separate variables             
            var (FileType, FileExtension) = _fileValidate.GetFileTypeExtension(FileUploaded.FileToUpload);

            try
            {
                //if meeting minute id is null, it does not exist so do a post request
                if (boardMeeting.MeetingMinuteId == null)
                {
                    baseAPIUrl = $"api/boardmeetings/{Id}/meetingminutes";

                    //generate the file name instead of letting the user provide one
                    FileName = $"MeetingMinute_{DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")}{FileExtension}";

                    //set the MeetingMinute filename here as otherwise it will be null
                    //this property is generated by the server and not provided by the user
                    var meetingToCreate = new MeetingMinuteInputDto { FileName = this.FileName };

                    //get the response with authorization as the API endpoint requires an authenticated user
                    var response = await (await _apiClient.WithAuthorization()).PostAsJsonAsync<MeetingMinuteInputDto>(baseAPIUrl,
                        meetingToCreate, cancellationToken);

                    //return the same page with an error message if the user is trying to call the API too many times
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                        return Partial("_UploadMeetingMinutePartial", this);
                    }

                    //ensure success status code else throw an exception
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    //meeting minute already exists so just need to upload the new file
                    //no need to update anything as we will re-use the existing file name
                    FileName = boardMeeting.FileName;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_UploadMeetingMinutePartial", this);
            }


            //MeetingMinute was created/updated successfully so can upload file to blob storage            
            var blobFileToUpload = _azureBlob.GetAzureBlobFileReference(FileName, AzureBlobFolder);

            //set the content type of the blob file so that downloads suggest correct file type to save as
            blobFileToUpload.Properties.ContentType = FileType;

            //upload the file to Azure
            await _azureBlob.UploadAzureBlobFileAsync(FileUploaded.FileToUpload, blobFileToUpload);

            //if successful, return the partial view which will have the IsValid value set to true by default
            //this is because ajax is used to post the form rather than a submit button 
            return Partial("_UploadMeetingMinutePartial", this);
        }

        public async Task<IActionResult> OnGetICALFile(CancellationToken cancellationToken)
        {
            var baseAPIUrl = $"api/boardmeetings/{Id}";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithGETOnlyAccessAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return RedirectToPage("/Index");
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                boardMeeting = streamContent.ReadAndDeserializeFromJson<BoardMeetingViewDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return RedirectToPage("/Index");
            }

            StringBuilder sb = new StringBuilder();
            string DateFormat = "yyyyMMddTHHmmssZ";
            string now = DateTime.Now.ToUniversalTime().ToString(DateFormat);
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("PRODID:-//GENERIC HOA//EN");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:PUBLISH");

            DateTime dtStart = Convert.ToDateTime(boardMeeting.ScheduledTime);
            DateTime dtEnd = Convert.ToDateTime(boardMeeting.ScheduledTime.AddHours(1));
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTART:" + dtStart.ToUniversalTime().ToString(DateFormat));
            sb.AppendLine("DTEND:" + dtEnd.ToUniversalTime().ToString(DateFormat));
            sb.AppendLine("DTSTAMP:" + now);
            sb.AppendLine("UID:" + Guid.NewGuid());
            sb.AppendLine("CREATED:" + now);
            sb.AppendLine("X-ALT-DESC;FMTTYPE=text/html:" + "HOA Board Meeting");
            sb.AppendLine("DESCRIPTION:" + boardMeeting.Description);
            sb.AppendLine("LAST-MODIFIED:" + now);
            sb.AppendLine("LOCATION:" + boardMeeting.ScheduledLocation);
            sb.AppendLine("SEQUENCE:0");
            sb.AppendLine("STATUS:CONFIRMED");
            sb.AppendLine("SUMMARY:" + boardMeeting.Title);
            sb.AppendLine("TRANSP:OPAQUE");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            byte[] calendarBytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(calendarBytes, "text/calendar", "hoameeting.ics");
        }
    }
}