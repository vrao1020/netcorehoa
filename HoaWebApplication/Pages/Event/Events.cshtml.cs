using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Extensions.Streams;
using HoaCommon.Extensions.UserClaims;
using HoaWebApplication.Models.ImageModels;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.OutputModels;
using HoaWebApplication.Models.PaginationModels;
using HoaEntities.Models.UpdateModels;
using HoaWebApplication.Services.Azure;
using HoaWebApplication.Services.File;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using PhotoSauce.MagicScaler;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace HoaWebApplication.Pages.Event
{
    public class EventsModel : PageModel
    {
        private ApiClient _apiClient;
        private string baseAPIUrl;
        private const string AzureBlobFolder = "eventimages"; //azure blob storage folder where files will be stored/fetched
        private IMapper _mapper;
        private IAzureBlob _azureBlob;
        private IFileValidate _fileValidate;
        private IAzureSASTokenUrl _azureSASTokenUrl;
        private ILogger<EventsModel> _logger;

        //Pagination parameters deserialized from API X-Pagination header. This will help paginate the data returned
        public XPaginationHeaderDto XPaginationDto;
        public IEnumerable<EventViewDto> eventsForView;
        public EventViewDto eventForView;
        public bool EditEvent = false;
        public bool UserIsAdmin = false;

        //image and thumbnail names used to upload to azure
        public string ImageName = null;
        public string ThumbnailName = null;

        //page number, page size, filters, and sorts that will be used for pagination
        [BindProperty(SupportsGet = true)]
        public PageParametersDto PageParameters { get; set; }

        //common property that will be used to map to input / update dtos
        [BindProperty]
        public EventManipulationDto EventToCreateEdit { get; set; }

        //file property for user file uploads
        //name in BindProperty is being explicitly passed in due to a bug (see https://github.com/JeremySkinner/FluentValidation/issues/1029)
        [BindProperty(Name = "ImageUploaded")]
        public OptionalImageDto ImageUploaded { get; set; }

        //used for binding to the id the specific Event that will be edited
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public EventsModel(ApiClient apiClient, IMapper mapper, IAzureBlob azureBlob, IFileValidate fileValidate,
            IAzureSASTokenUrl azureSASTokenUrl, ILogger<EventsModel> logger)
        {
            _apiClient = apiClient;
            _mapper = mapper;
            _azureBlob = azureBlob;
            _fileValidate = fileValidate;
            _azureSASTokenUrl = azureSASTokenUrl;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            TempData["PageNum"] = PageParameters.PageNum;
            TempData["PageSize"] = PageParameters.PageSize;

            baseAPIUrl = $"api/events?page={PageParameters.PageNum}&pagesize={PageParameters.PageSize}";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithGETOnlyAccessAuthorization()).GetAsync(baseAPIUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

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
                eventsForView = streamContent.ReadAndDeserializeFromJson<IEnumerable<EventViewDto>>();

                foreach (var item in eventsForView)
                {
                    if (item.ThumbnailName != null)
                    {
                        item.ThumbnailName = _azureSASTokenUrl.GetImageUrlWithSAS(item.ThumbnailName, AzureBlobFolder);
                    }
                }
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

        public IActionResult OnGetFetchCreateEventPartial()
        {
            //need to initialize an empty object because of using modals to create and edit data
            //in MVC, you can return a partial and send along a model to the partial view
            //this will ensure that both model state errors and posted values are sent back to the end user
            //in razor pages, if you only send the model (i.e. this.property), model state errors are lost
            //instead, you have to send the page as the model (i.e. this refers to the pagemodel as a whole)
            //and set the value of the fields on the form to be fetched from the property the form is posting to - 
            //see example on how this is done on this page and the partial page it pulls from
            EventToCreateEdit = new EventManipulationDto();

            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            //call the partial page and pass in the current instance of the MeetingMinuteModel class
            //this keyword refers to the current instance of this class
            //this is required as we need to check the values of some 
            return Partial("_CreateEditEventPartial", this);
        }

        public async Task<IActionResult> OnPostCreateEventAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_CreateEditEventPartial", this);
            }

            var baseAPIUrl = $"api/events";

            //map the manipulationdto to an inputdto
            var eventToCreate = _mapper.Map<EventInputDto>(EventToCreateEdit);

            //initialize a filename and filetype variables
            //these will hold the correct values if the user uploaded an image
            var (FileType, FileExtension) = (String.Empty, String.Empty);

            if (ImageUploaded.FileToUpload != null)
            {
                //GetFileTypeExtension returns a tuple that is deconstructed into separate variables             
                (FileType, FileExtension) = _fileValidate.GetImageTypeExtension(ImageUploaded.FileToUpload);

                //generate the file name instead of letting the user provide one
                ImageName = $"Event_Original_{DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")}{FileExtension}";

                //generate the file name instead of letting the user provide one
                ThumbnailName = $"Event_Thumbnail_{DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")}{FileExtension}";

                //set the Image name
                eventToCreate.ImageName = ImageName;
                //set the Thumbnail name
                eventToCreate.ThumbnailName = ThumbnailName;
            }

            try
            {
                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PostAsJsonAsync<EventInputDto>(baseAPIUrl,
                    eventToCreate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditEventPartial", this);
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
                return Partial("_CreateEditEventPartial", this);
            }

            //if user uploaded the file, upload the file to azure
            if (ImageUploaded.FileToUpload != null)
            {
                try
                {
                    await UploadImagesAsync(FileType);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                    //either the API is not running or an error occured on the server
                    TempData["Error"] = "An error occured while processing your request. Please try again later.";
                    return Partial("_CreateEditEventPartial", this);
                }
            }

            //if successful, return the partial view which will have the IsValid value set to true by default
            //this is because ajax is used to post the form rather than a submit button 
            return Partial("_CreateEditEventPartial", this);
        }

        public async Task<IActionResult> OnGetFetchEditEventPartialAsync(CancellationToken cancellationToken)
        {
            //set this value to true as the partial page will include specific 
            //elements in the modal based on this value
            EditEvent = true;

            var baseAPIUrl = $"api/events/{Id}";

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
                    return Partial("_CreateEditEventPartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                eventForView = streamContent.ReadAndDeserializeFromJson<EventViewDto>();

                //if image name is not null, proceed. Otherwise user never uploaded an image
                if (eventForView.ImageName != null)
                {
                    //store the image names so that if user submits a new image, blob storage retains the same file name
                    TempData["ImageUrl"] = eventForView.ImageName;
                    TempData["ThumbnailUrl"] = eventForView.ThumbnailName;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_CreateEditEventPartial", this);
            }

            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            return Partial("_CreateEditEventPartial", this);
        }

        public async Task<IActionResult> OnPostEditEventAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_CreateEditEventPartial", this);
            }

            var baseAPIUrl = $"api/events/{Id}";

            //map the manipulationdto to an updatedto
            var eventToUpdate = _mapper.Map<EventUpdateDto>(EventToCreateEdit);

            //initialize a filename and filetype variables
            //these will hold the correct values if the user uploaded an image
            var (FileType, FileExtension) = (String.Empty, String.Empty);

            if (ImageUploaded.FileToUpload != null)
            {
                //GetFileTypeExtension returns a tuple that is deconstructed into separate variables             
                (FileType, FileExtension) = _fileValidate.GetImageTypeExtension(ImageUploaded.FileToUpload);

                //if tempdata contains the image url, get the stored values as we won't want the file names to change
                if (TempData["ImageUrl"] as string != null)
                {
                    ImageName = TempData["ImageUrl"] as string;
                    ThumbnailName = TempData["ThumbnailUrl"] as string;
                }
                else //no value so generate new ones
                {
                    //generate the file name instead of letting the user provide one
                    ImageName = $"Event_Original_{DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")}{FileExtension}";

                    //generate the file name instead of letting the user provide one
                    ThumbnailName = $"Event_Thumbnail_{DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")}{FileExtension}";
                }

                //set the Image name
                eventToUpdate.ImageName = ImageName;
                //set the Thumbnail name
                eventToUpdate.ThumbnailName = ThumbnailName;
            }

            try
            {
                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PutAsJsonAsync<EventUpdateDto>(baseAPIUrl,
                    eventToUpdate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditEventPartial", this);
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
                return Partial("_CreateEditEventPartial", this);
            }

            //if user uploaded the file, upload/update the file to azure
            if (ImageUploaded.FileToUpload != null)
            {
                try
                {
                    await UploadImagesAsync(FileType);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                    //either the API is not running or an error occured on the server
                    TempData["Error"] = "An error occured while processing your request. Please try again later.";
                    return Partial("_CreateEditEventPartial", this);
                }
            }

            return Partial("_CreateEditEventPartial", this);
        }

        public async Task<IActionResult> OnPostDeleteEventAsync(CancellationToken cancellationToken)
        {
            var baseAPIUrl = $"api/events/{Id}";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var responseForGet = await (await _apiClient.WithAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (responseForGet.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Content("Error");
                }

                //ensure success status code else throw an exception
                responseForGet.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await responseForGet.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                eventForView = streamContent.ReadAndDeserializeFromJson<EventViewDto>();

                //set the file name from the fetched meeting minute
                ImageName = eventForView.ImageName;
                ThumbnailName = eventForView.ThumbnailName;

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

                if (ImageName != null)
                {
                    //deleting was successful as we checked for a success status code
                    //delete the images from blob storage as well
                    var imageToDelete = _azureBlob.GetAzureBlobFileReference(ImageName, AzureBlobFolder);
                    var thumbnailToDelete = _azureBlob.GetAzureBlobFileReference(ThumbnailName, AzureBlobFolder);

                    //we want to delete both blobs in parallel
                    //if we used await _azureblob.deletefile(image), it would block the program from executing
                    //the other await statement until the first finished
                    //------------------------
                    //don't do below as it will block the second method from executing until the first has completed
                    //await _azureBlob.DeleteAzureBlobFileAsync(imageToDelete);
                    //await _azureBlob.DeleteAzureBlobFileAsync(thumbnailToDelete);
                    //----------------------------
                    //we first initialize the tasks to immediately start executing by assining them to a variable
                    //this will return a task that can be awaited to ensure it completes before we return from this action
                    //this ensures both delete functions execute in parallel as we kicked them off first before awaiting
                    var deleteImageTask = _azureBlob.DeleteAzureBlobFileAsync(imageToDelete);
                    var deleteThumbnailTask = _azureBlob.DeleteAzureBlobFileAsync(thumbnailToDelete);
                    await deleteImageTask;
                    await deleteThumbnailTask;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Content("Error");
            }

            return Content("Success");
        }

        public async Task<IActionResult> OnGetICALFile(CancellationToken cancellationToken)
        {
            var baseAPIUrl = $"api/events/{Id}";

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
                eventForView = streamContent.ReadAndDeserializeFromJson<EventViewDto>();
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

            DateTime dtStart = Convert.ToDateTime(eventForView.ScheduledTime);
            DateTime dtEnd = Convert.ToDateTime(eventForView.ScheduledTime.AddHours(1));
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTART:" + dtStart.ToUniversalTime().ToString(DateFormat));
            sb.AppendLine("DTEND:" + dtEnd.ToUniversalTime().ToString(DateFormat));
            sb.AppendLine("DTSTAMP:" + now);
            sb.AppendLine("UID:" + Guid.NewGuid());
            sb.AppendLine("CREATED:" + now);
            sb.AppendLine("X-ALT-DESC;FMTTYPE=text/html:" + "HOA Event");
            sb.AppendLine("DESCRIPTION:" + eventForView.Message);
            sb.AppendLine("LAST-MODIFIED:" + now);
            sb.AppendLine("LOCATION:" + "N/A");
            sb.AppendLine("SEQUENCE:0");
            sb.AppendLine("STATUS:CONFIRMED");
            sb.AppendLine("SUMMARY:" + eventForView.Title);
            sb.AppendLine("TRANSP:OPAQUE");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            byte[] calendarBytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(calendarBytes, "text/calendar", "hoaevent.ics");
        }

        private async Task UploadImagesAsync(string fileType)
        {
            //create a stream so that the image can be read to be resized
            using (var memoryStream = new MemoryStream())
            {
                //read the file into the stream
                await ImageUploaded.FileToUpload.CopyToAsync(memoryStream);
                //set the position to 0 so that the file is not empty
                memoryStream.Position = 0;

                //create another temp stream that will be used to hold the resized image
                using (var outStream = new MemoryStream())
                {
                    //resize the image uploaded into the correct size
                    MagicImageProcessor.ProcessImage(memoryStream, outStream,
                        new ProcessImageSettings
                        {
                            Width = 400,
                            Height = 400,
                            ResizeMode = CropScaleMode.Crop,
                            HybridMode = HybridScaleMode.FavorSpeed
                        });

                    //set the position to 0 so that the file is not empty
                    outStream.Position = 0;

                    //get a reference to the file in Azure
                    var blobFileToUpload = _azureBlob.GetAzureBlobFileReference(ImageName, AzureBlobFolder);

                    //set the content type of the blob file so that downloads suggest correct file type to save as
                    blobFileToUpload.Properties.ContentType = fileType;

                    //upload the original resized image
                    await _azureBlob.UploadAzureBlobFileAsync(outStream, blobFileToUpload);
                }

                //create another temp stream that will be used to hold the thumbnail image
                using (var outStream = new MemoryStream())
                {
                    //set the position to 0 so that the file is not empty
                    memoryStream.Position = 0;

                    //resize the image uploaded into the correct size
                    MagicImageProcessor.ProcessImage(memoryStream, outStream,
                        new ProcessImageSettings
                        {
                            Width = 200,
                            Height = 200,
                            ResizeMode = CropScaleMode.Crop,
                            HybridMode = HybridScaleMode.FavorSpeed
                        });

                    //set the position to 0 so that the file is not empty
                    outStream.Position = 0;

                    //get a reference to the file in Azure
                    var blobFileToUpload = _azureBlob.GetAzureBlobFileReference(ThumbnailName, AzureBlobFolder);

                    //set the content type of the blob file so that downloads suggest correct file type to save as
                    blobFileToUpload.Properties.ContentType = fileType;

                    //upload the original resized image
                    await _azureBlob.UploadAzureBlobFileAsync(outStream, blobFileToUpload);
                }
            }
        }
    }
}

//skiasharp
//using (var original = SKBitmap.Decode(memoryStream))
//{
//    using (var resized = original
//           .Resize(new SKImageInfo(400, 400), SKFilterQuality.None))
//    {
//        using (var image = SKImage.FromBitmap(resized))
//        {
//            using (var stream = new FileStream("C:\\Users\\VNelakonda\\Desktop\\" + "resized.jpg", FileMode.Create))
//            {
//                image.Encode(SKEncodedImageFormat.Jpeg, 75).SaveTo(stream);
//            }
//        }
//    }
//}

//drawing.common
//using (var image = new Bitmap(memoryStream))
//{
//    var resized = new Bitmap(400, 400);
//    using (var graphics = Graphics.FromImage(resized))
//    {
//        graphics.CompositingQuality = CompositingQuality.HighSpeed;
//        graphics.InterpolationMode = InterpolationMode.Default;
//        graphics.CompositingMode = CompositingMode.SourceCopy;
//        graphics.DrawImage(image, 0, 0, 400, 400);
//        resized.Save("C:\\Users\\VNelakonda\\Desktop\\" + "resized.jpg", ImageFormat.Jpeg);
//    }
//}

//imagesharp
//using (Image<Rgba32> image = Image.Load(memoryStream)) //open the file and detect the file type and decode it
//{
//    // image is now in a file format agnostic structure in memory as a series of Rgba32 pixels
//    image.Mutate(ctx => ctx.Resize(400, 400)); // resize the image in place and return it for chaining
//    image.Save("C:\\Users\\VNelakonda\\Desktop\\" + "resized.jpg"); // based on the file extension pick an encoder then encode and write the data to disk
//} // dispose - releasing memory into a memory pool ready for the next image you wish to process      