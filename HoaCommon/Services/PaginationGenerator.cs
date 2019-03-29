using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;

namespace HoaCommon.Services
{
    public class PaginationGenerator : IPaginationGenerator
    {
        private IHttpContextAccessor _httpContextAccessor;
        private IUrlHelperFactory _urlHelperFactory;
        private IActionContextAccessor _actionContextAccessor;
        private Dictionary<string, string> routeNameValues = new Dictionary<string, string>
        {
            {"Events", "GetEvents"},
            {"Posts", "GetPosts"},
            {"MeetingMinutes", "GetMeetingMinutes"},
            {"Users", "GetUsers"},
            {"BoardMeetings", "GetBoardMeetings"},
            {"Comments", "GetComments"},
            {"UserClaims", "GetUsers"}
        };

        public PaginationGenerator(IHttpContextAccessor httpContextAccessor, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public void GenerateHeaders(int totalCount, int pageSize, int pageNumber, string sorts, string filters)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            bool prevPage = pageNumber > 1;
            bool nextPage = pageNumber < totalPages;

            var previousPageLink = prevPage ?
                CreateLink("Previous", pageNumber, pageSize, sorts, filters) : null;

            var nextPageLink = nextPage ?
                CreateLink("Next", pageNumber, pageSize, sorts, filters) : null;

            var paginationMetadata = new
            {
                totalCount = totalCount,
                pageSize = pageSize,
                currentPage = pageNumber,
                totalPages = totalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            _httpContextAccessor.HttpContext.Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
        }

        private string CreateLink(string pageType, int pageNumber, int pageSize, string sorts, string filters)
        {
            //urlhelperfactory requires the action context to create a URL
            //there is no method that gets a IUrlHelper without passing in the action context
            //action context is fetched via DI using IActionContextAccessor and injecting the required service in startup.cs
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            //you can fetch the controller name via the action context. There is no need to use a seperate service to fetch what
            //controller is currently requesting data
            var controllerName = _actionContextAccessor.ActionContext.RouteData.Values["controller"].ToString();

            //get the route name for the action from the dictionary so that the url helper can create a link to the appropriate page
            var routeName = routeNameValues[controllerName];

            switch (pageType)
            {
                case "Previous":
                    return urlHelper.Link(routeName,
                      new
                      {
                          page = pageNumber - 1,
                          pageSize = pageSize,
                          sorts = sorts,
                          filters = filters
                      });
                case "Next":
                    return urlHelper.Link(routeName,
                      new
                      {
                          page = pageNumber + 1,
                          pageSize = pageSize,
                          sorts = sorts,
                          filters = filters
                      });
                default:
                    return urlHelper.Link(routeName,
                    new
                    {
                        page = pageNumber,
                        pageSize = pageSize,
                        sorts = sorts,
                        filters = filters
                    });
            }
        }
    }
}
