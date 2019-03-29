using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Extensions.Streams;
using HoaCommon.Extensions.UserClaims;
using HoaEntities.Models.ManipulationModels;
using HoaEntities.Models.OutputModels;
using HoaWebApplication.Models.PaginationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using HoaEntities.Models.InputModels;
using System.Net;
using HoaEntities.Models.UpdateModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace HoaWebApplication.Pages.Post.Comment
{
    public class CommentsModel : PageModel
    {
        private string baseAPIUrl;
        private ApiClient _apiClient;
        private IMapper _mapper;
        private ILogger<CommentsModel> _logger;

        //Pagination parameters deserialized from API X-Pagination header. This will help paginate the data returned
        public XPaginationHeaderDto XPaginationDto;
        public IEnumerable<CommentViewDto> CommentsForView;
        public CommentViewDto CommentForView;
        public PostViewDto PostForView;
        public bool EditComment = false;
        public bool UserIsAdmin = false;

        //page number, page size, filters, and sorts that will be used for pagination
        [BindProperty(SupportsGet = true)]
        public PageParametersDto PageParameters { get; set; }

        //common property that will be used to map to input / update dtos
        [BindProperty]
        public CommentManipulationDto CommentToCreateEdit { get; set; }

        //used for binding to the id the specific Comment that will be edited
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        //used for binding to the id of the specific Post for which comments are created/edited/deleted
        [BindProperty(SupportsGet = true)]
        public Guid PostId { get; set; }

        public CommentsModel(ApiClient apiClient, IMapper mapper, ILogger<CommentsModel> logger)
        {
            _apiClient = apiClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            TempData["PageNum"] = PageParameters.PageNum;
            TempData["PageSize"] = PageParameters.PageSize;

            baseAPIUrl = $"api/Posts/{PostId}";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithGETOnlyAccessAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Page();
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                PostForView = streamContent.ReadAndDeserializeFromJson<PostViewDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Page();
            }

            //change base api url to fetch comments
            baseAPIUrl = $"api/Posts/{PostId}/Comments";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithGETOnlyAccessAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
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
                CommentsForView = streamContent.ReadAndDeserializeFromJson<IEnumerable<CommentViewDto>>();
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

        public IActionResult OnGetFetchCreateCommentPartial()
        {
            //need to initialize an empty object because of using modals to create and edit data
            //in MVC, you can return a partial and send along a model to the partial view
            //this will ensure that both model state errors and posted values are sent back to the end user
            //in razor pages, if you only send the model (i.e. this.property), model state errors are lost
            //instead, you have to send the page as the model (i.e. this refers to the pagemodel as a whole)
            //and set the value of the fields on the form to be fetched from the property the form is posting to - 
            //see example on how this is done on this page and the partial page it pulls from
            CommentToCreateEdit = new CommentManipulationDto();

            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            //call the partial page and pass in the current instance of the PostMinuteModel class
            //this keyword refers to the current instance of this class
            //this is required as we need to check the values of some 
            return Partial("_CreateEditCommentPartial", this);
        }

        public async Task<IActionResult> OnPostCreateCommentAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_CreateEditCommentPartial", this);
            }

            baseAPIUrl = $"api/posts/{PostId}/comments";

            try
            {
                var commentToCreate = _mapper.Map<CommentInputDto>(CommentToCreateEdit);

                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PostAsJsonAsync<CommentInputDto>(baseAPIUrl,
                    commentToCreate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditCommentPartial", this);
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
                return Partial("_CreateEditCommentPartial", this);
            }

            //if successful, return the partial view which will have the IsValid value set to true by default
            //this is because ajax is used to comment the form rather than a submit button 
            return Partial("_CreateEditCommentPartial", this);
        }

        public async Task<IActionResult> OnGetFetchEditCommentPartialAsync(CancellationToken cancellationToken)
        {
            //set this value to true as the partial page will include specific 
            //elements in the modal based on this value
            EditComment = true;

            baseAPIUrl = $"api/posts/{PostId}/Comments/{Id}";

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
                    return Partial("_CreateEditCommentPartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                CommentForView = streamContent.ReadAndDeserializeFromJson<CommentViewDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_CreateEditCommentPartial", this);
            }

            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            return Partial("_CreateEditCommentPartial", this);
        }

        public async Task<IActionResult> OnPostEditCommentAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_CreateEditCommentPartial", this);
            }

            baseAPIUrl = $"api/posts/{PostId}/Comments/{Id}";

            try
            {
                var commentToUpdate = _mapper.Map<CommentUpdateDto>(CommentToCreateEdit);

                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PutAsJsonAsync<CommentUpdateDto>(baseAPIUrl,
                    commentToUpdate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditCommentPartial", this);
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
                return Partial("_CreateEditCommentPartial", this);
            }

            return Partial("_CreateEditCommentPartial", this);
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(CancellationToken cancellationToken)
        {
            baseAPIUrl = $"api/posts/{PostId}/Comments/{Id}";

            try
            {
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

            return Content("Success");
        }
    }
}