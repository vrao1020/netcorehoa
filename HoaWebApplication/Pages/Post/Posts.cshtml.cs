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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace HoaWebApplication.Pages.Post
{
    public class PostsModel : PageModel
    {
        private string baseAPIUrl;
        private ApiClient _apiClient;
        private IMapper _mapper;
        private ILogger<PostsModel> _logger;

        //Pagination parameters deserialized from API X-Pagination header. This will help paginate the data returned
        public XPaginationHeaderDto XPaginationDto;
        public IEnumerable<PostViewDto> posts;
        public PostViewDto post;
        public bool EditPost = false;
        public bool UserIsAdmin = false;

        //page number, page size, filters, and sorts that will be used for pagination
        [BindProperty(SupportsGet = true)]
        public PageParametersDto PageParameters { get; set; }

        //common property that will be used to map to input / update dtos
        [BindProperty]
        public PostManipulationDto PostToCreateEdit { get; set; }

        //used for binding to the id the specific Post that will be edited
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public PostsModel(ApiClient apiClient, IMapper mapper, ILogger<PostsModel> logger)
        {
            _apiClient = apiClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            TempData["PageNum"] = PageParameters.PageNum;
            TempData["PageSize"] = PageParameters.PageSize;

            baseAPIUrl = $"api/Posts?page={PageParameters.PageNum}&pagesize={PageParameters.PageSize}";

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
                posts = streamContent.ReadAndDeserializeFromJson<IEnumerable<PostViewDto>>();
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

        public IActionResult OnGetFetchCreatePostPartial()
        {
            //need to initialize an empty object because of using modals to create and edit data
            //in MVC, you can return a partial and send along a model to the partial view
            //this will ensure that both model state errors and posted values are sent back to the end user
            //in razor pages, if you only send the model (i.e. this.property), model state errors are lost
            //instead, you have to send the page as the model (i.e. this refers to the pagemodel as a whole)
            //and set the value of the fields on the form to be fetched from the property the form is posting to - 
            //see example on how this is done on this page and the partial page it pulls from
            PostToCreateEdit = new PostManipulationDto();

            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            //call the partial page and pass in the current instance of the PostMinuteModel class
            //this keyword refers to the current instance of this class
            //this is required as we need to check the values of some 
            return Partial("_CreateEditPostPartial", this);
        }

        public async Task<IActionResult> OnPostCreatePostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_CreateEditPostPartial", this);
            }

            var baseAPIUrl = $"api/Posts";

            try
            {
                var postToCreate = _mapper.Map<PostInputDto>(PostToCreateEdit);

                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PostAsJsonAsync<PostInputDto>(baseAPIUrl,
                    postToCreate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditPostPartial", this);
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
                return Partial("_CreateEditPostPartial", this);
            }

            //if successful, return the partial view which will have the IsValid value set to true by default
            //this is because ajax is used to post the form rather than a submit button 
            return Partial("_CreateEditPostPartial", this);
        }

        public async Task<IActionResult> OnGetFetchEditPostPartialAsync(CancellationToken cancellationToken)
        {
            //set this value to true as the partial page will include specific 
            //elements in the modal based on this value
            EditPost = true;

            var baseAPIUrl = $"api/Posts/{Id}";

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
                    return Partial("_CreateEditPostPartial", this);
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                post = streamContent.ReadAndDeserializeFromJson<PostViewDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Partial("_CreateEditPostPartial", this);
            }

            //see startup in AddMvc for how this partial is fetched
            //this was an answer found at https://softdevpractice.com/blog/asp-net-core-mvc-ajax-modals/
            return Partial("_CreateEditPostPartial", this);
        }

        public async Task<IActionResult> OnPostEditPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Partial("_CreateEditPostPartial", this);
            }

            var baseAPIUrl = $"api/Posts/{Id}";

            try
            {
                var postToUpdate = _mapper.Map<PostUpdateDto>(PostToCreateEdit);

                //get the response with authorization as the API endpoint requires an authenticated user
                var response = await (await _apiClient.WithAuthorization()).PutAsJsonAsync<PostUpdateDto>(baseAPIUrl,
                    postToUpdate, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Partial("_CreateEditPostPartial", this);
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
                return Partial("_CreateEditPostPartial", this);
            }

            return Partial("_CreateEditPostPartial", this);
        }

        public async Task<IActionResult> OnPostDeletePostAsync(CancellationToken cancellationToken)
        {
            var baseAPIUrl = $"api/Posts/{Id}";

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