﻿using AutoMapper;
using HoaEntities.Models.OutputModels;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Extensions.Streams;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace HoaWebApplication.ViewComponents
{
    public class BoardMeetingViewComponent : ViewComponent
    {
        private ApiClient _apiClient;
        private IMapper _mapper;
        private ILogger<BoardMeetingViewComponent> _logger;
        private BoardMeetingViewDto BoardMeetingForView;

        public BoardMeetingViewComponent(ApiClient apiClient, IMapper mapper, ILogger<BoardMeetingViewComponent> logger)
        {
            _apiClient = apiClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var baseAPIUrl = $"api/BoardMeetings/GetCurrentBoardMeetingDue";

            try
            {
                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithGETOnlyAccessAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return View(new BoardMeetingViewDto());
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                BoardMeetingForView = streamContent.ReadAndDeserializeFromJson<BoardMeetingViewDto>();

                if (BoardMeetingForView == null)
                {
                    return View(new BoardMeetingViewDto());
                }

                //cut down the size of the message if its longer than 75 characters
                if (BoardMeetingForView.Description.Length > 75)
                {
                    BoardMeetingForView.Description = BoardMeetingForView.Description.Substring(0, 75) + "...";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return View(new BoardMeetingViewDto());
            }


            return View(BoardMeetingForView);
        }
    }
}
