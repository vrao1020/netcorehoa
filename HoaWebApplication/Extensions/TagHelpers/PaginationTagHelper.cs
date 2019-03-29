using HoaWebApplication.Models.PaginationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace HoaWebApplication.Extensions.TagHelpers
{
    public class PaginationTagHelper : TagHelper
    {
        public XPaginationHeaderDto XPaginationDto { get; set; }
        public string RouteUrl { get; set; }

        //if a large amount of pages exist, only display a max of 5 pages
        const int maxDisplayedPages = 5;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //if only 1 page exists, there is no need to display any pagination data
            if (XPaginationDto.TotalPages == 0 || XPaginationDto.TotalPages == 1)
            {
                return;
            }

            //initialize a nav tag name that will be the basis of the pagination
            output.TagName = "nav";
            output.Attributes.Add("aria-label", "Page navigation");
            output.PreContent.SetHtmlContent(@"<ul class=""pagination"">");

            //create a go to first page link that is not disabled as user is not on first page
            if (XPaginationDto.CurrentPage != 1)
            {
                var linkToAdd = CreatePagingLink(1, RouteUrl, false, false, false, false, true, false);
                output.Content.AppendHtml(linkToAdd);
            }
            else //create a go to first page link that is disabled as user is on first page
            {
                var linkToAdd = CreatePagingLink(1, RouteUrl, true, false, false, false, true, false);
                output.Content.AppendHtml(linkToAdd);
            }

            //create a go to previous page link that is not disabled as prior link exists 
            if (XPaginationDto.PreviousPageLink != null)
            {
                var linkToAdd = CreatePagingLink(XPaginationDto.CurrentPage - 1, RouteUrl, false, false, true, false, false, false);
                output.Content.AppendHtml(linkToAdd);
            }
            else //create a go to previous page link that is disabled as prior link does not exist
            {
                var linkToAdd = CreatePagingLink(XPaginationDto.CurrentPage - 1, RouteUrl, true, false, true, false, false, false);
                output.Content.AppendHtml(linkToAdd);
            }

            //get the start and ending page numbers
            //this method will calculate based on total pages, how many pages to display to the user
            //max displayed pages will limit the amount of pages showed to the user
            var (startPageNum, endPageNum) = CalculatePagesToShow(XPaginationDto.CurrentPage, XPaginationDto.TotalPages, maxDisplayedPages);

            //if start page is not 1, then user is on a page where they might want to go pack to the first page
            //this creates a link to the first page and also displays a ... for user visual
            if (startPageNum > 1)
            {
                var linkToAdd = CreatePagingLink(1, RouteUrl, false, false, false, false, false, false);
                output.Content.AppendHtml(linkToAdd);

                var gapTag = new TagBuilder("li");
                gapTag.AddCssClass("page-item border-0");
                //gapTag.InnerHtml.AppendHtml("&nbsp;...&nbsp;");
                gapTag.InnerHtml.AppendHtml("<p>...</p>");
                output.Content.AppendHtml(gapTag);
            }

            for (var i = startPageNum; i <= endPageNum; i++)
            {
                //loop through the number of pages and create page links 
                if (i == XPaginationDto.CurrentPage)
                {
                    var linkToAdd = CreatePagingLink(i, RouteUrl, false, true, false, false, false, false);
                    output.Content.AppendHtml(linkToAdd);
                }
                else
                {
                    var linkToAdd = CreatePagingLink(i, RouteUrl, false, false, false, false, false, false);
                    output.Content.AppendHtml(linkToAdd);
                }
            }

            //if end page is not 1, then user is on a page where they might want to go pack to the first page
            //this creates a link to the first page and also displays a ... for user visual
            if (endPageNum < XPaginationDto.TotalPages)
            {
                var gapTag = new TagBuilder("li");
                gapTag.AddCssClass("page-item border-0");
                //gapTag.InnerHtml.AppendHtml("&nbsp;...&nbsp;");
                gapTag.InnerHtml.AppendHtml("<div>...</div>");
                output.Content.AppendHtml(gapTag);

                var linkToAdd = CreatePagingLink(XPaginationDto.TotalPages, RouteUrl, false, false, false, false, false, false);
                output.Content.AppendHtml(linkToAdd);
            }

            //create a go to next page link that is not disabled as next page link exists 
            if (XPaginationDto.NextPageLink != null)
            {
                var linkToAdd = CreatePagingLink(XPaginationDto.CurrentPage + 1, RouteUrl, false, false, false, true, false, false);
                output.Content.AppendHtml(linkToAdd);
            }
            else //create a go to next page link that is disabled as next page link does not exist
            {
                var linkToAdd = CreatePagingLink(XPaginationDto.CurrentPage + 1, RouteUrl, true, false, false, true, false, false);
                output.Content.AppendHtml(linkToAdd);
            }

            //create a go to last page link that is not disabled as user is not on last page
            if (XPaginationDto.CurrentPage != XPaginationDto.TotalPages)
            {
                var linkToAdd = CreatePagingLink(XPaginationDto.TotalPages, RouteUrl, false, false, false, false, false, true);
                output.Content.AppendHtml(linkToAdd);
            }
            else //create a go to last page link that is disabled as user is on last page
            {
                var linkToAdd = CreatePagingLink(XPaginationDto.TotalPages, RouteUrl, true, false, false, false, false, true);
                output.Content.AppendHtml(linkToAdd);
            }

            output.PostContent.SetHtmlContent("</ul>");

            //append total record / total pages information          
            output.PostContent.AppendHtml(AddDisplayInfo());
        }

        /// <summary>
        /// Creates a navigation link. Has many options to create various types of nav links based on boolean values passed in
        /// </summary>
        /// <param name="targetPageNo">Page number to navigate user to</param>
        /// <param name="route">Page to navigate user to</param>
        /// <param name="isDisabled">If true, link will be disabled</param>
        /// <param name="isCurrent">If true, link will be set to active</param>
        /// <param name="createPrevious">If true, will create a previous page link</param>
        /// <param name="createNext">If true, will create a next page link</param>
        /// <param name="createGoToFirst">If true, will create a go to first page link</param>
        /// <param name="createGoToLast">If true, will create a go to last page link</param>
        /// <returns></returns>
        private TagBuilder CreatePagingLink(int targetPageNo, string route, bool isDisabled,
            bool isCurrent, bool createPrevious, bool createNext, bool createGoToFirst, bool createGoToLast)
        {
            //initialize a list item
            var liTag = new TagBuilder("li");
            liTag.AddCssClass("page-item");

            //in case there is no previous/next link, disable the link
            if (isDisabled)
            {
                liTag.AddCssClass("disabled");
            }

            //initialize a link
            var aTag = new TagBuilder("a");
            aTag.AddCssClass("page-link");

            //set the href to point to the route along with the page parameters
            //note that the actual page that the link targets is based on the targetPageNo value that is passed in
            aTag.Attributes.Add("href", $"{RouteUrl}/{targetPageNo}?pageSize={XPaginationDto.PageSize}");

            //if true, set the current tag to be active via a css class
            if (isCurrent)
            {
                liTag.AddCssClass("active");
                aTag.InnerHtml.AppendHtml(@"<span class=""sr-only"">(current)</span>");
            }

            //once the link tag is created, the attributes of the tag and the content of the tag are all based
            //on boolean values that are passed in
            //only one type of tag will be created (i.e. a previous link, next link, regular page link, etc.)

            //if true, create a previous tag that goes to the prior page
            if (createPrevious)
            {
                aTag.Attributes.Add("tabindex", "-1");
                aTag.Attributes.Add("aria-label", "Previous");
                aTag.InnerHtml.AppendHtml(@"<span aria-hidden=""true"">&lsaquo;</span>");
                aTag.InnerHtml.AppendHtml(@"<span class=""sr-only"">Previous</span>");
            } //if true, create a next tag that goes to the prior page
            else if (createNext)
            {
                aTag.Attributes.Add("aria-label", "Next");
                aTag.InnerHtml.AppendHtml(@"<span aria-hidden=""true"">&rsaquo;</span>");
                aTag.InnerHtml.AppendHtml(@"<span class=""sr-only"">Next</span>");
            } //if true, create a go to first page tag that goes to the first page
            else if (createGoToFirst)
            {
                aTag.Attributes.Add("aria-label", "Go To First Page");
                aTag.InnerHtml.AppendHtml(@"<span aria-hidden=""true"">&laquo;</span>");
                aTag.InnerHtml.AppendHtml(@"<span class=""sr-only"">Go To First Page</span>");
            } //if true, create a go to last page tag that goes to the last page
            else if (createGoToLast)
            {
                aTag.Attributes.Add("aria-label", "Go To Last Page");
                aTag.InnerHtml.AppendHtml(@"<span aria-hidden=""true"">&raquo;</span>");
                aTag.InnerHtml.AppendHtml(@"<span class=""sr-only"">Go To Last Page</span>");
            }
            else //is a regular tag (i.e. just the page number)
            {
                aTag.InnerHtml.Append($"{targetPageNo}");
            }

            //append the link tag to the list item
            //each link tag is a seperate link
            liTag.InnerHtml.AppendHtml(aTag);

            return liTag;
        }

        /// <summary>
        /// Calculates the pages to show based on total pages
        /// This prevents a long list of pages being shown
        /// </summary>
        /// <param name="currentPageNo"></param>
        /// <param name="totalPages"></param>
        /// <param name="maxDisplayedPages"></param>
        /// <returns></returns>
        private (int startPageNum, int endPageNum) CalculatePagesToShow(int currentPageNo, int totalPages, int maxDisplayedPages)
        {
            //set the starting page to 1
            var _start = 1;
            //sets the max amount of pages that are displayed (e.g. 1....20,21,22,23,24...50)
            var _end = maxDisplayedPages;
            //calculate the gap that sets the min amount of pages that need to exist for a gap to appear
            //e.g. 50 pages exist, then gap will appear
            //if only 3 pages exist, then don't show gap as gap is less than max pages to display
            var _gap = (int)Math.Ceiling(maxDisplayedPages / 2.0);

            //if max pages displayed is greater than total pages, show all the pages
            if (maxDisplayedPages > totalPages)
            {
                maxDisplayedPages = totalPages;
            }

            // << < 1 2 (3) 4 5 6 7 8 9 10 > >>
            //in this case, the current page is less than max displayed pages
            //so only display from 1 to max displayed pages (in this case 5)
            if (currentPageNo < maxDisplayedPages)
            {
                _start = 1;
                _end = maxDisplayedPages;
            }

            // << < 91 92 93 94 95 96 97 (98) 99 100 > >>
            //user is on a page that is close to the end, so ensure that the end is set to the total pages that exist
            //as we don't want to display the entire range of pages, set the start to be calculated based on below formula
            //in this case, 100 pages exist so 100 - 5 > 0 so start is 95 and end is 100           
            else if (currentPageNo + maxDisplayedPages > totalPages)
            {
                _start = totalPages - maxDisplayedPages > 0 ? totalPages - maxDisplayedPages : 1;
                _end = totalPages;
            }

            // << < 21 22 23 34 (25) 26 27 28 29 30 > >>
            //user is in the middle somewhere so set the end to be starting page + max displayed
            //starting pages is calculated based on the current page the user is on            
            else
            {
                _start = currentPageNo - _gap > 0 ? currentPageNo - _gap : 1;
                _end = _start + maxDisplayedPages;
            }

            return (_start, _end);
        }

        //add total page/record count information
        private TagBuilder AddDisplayInfo()
        {
            var infoDiv = new TagBuilder("div");

            //create a tag that has total pages that was returned by the api
            var totalPages = new TagBuilder("span");
            totalPages.AddCssClass("badge badge-primary mr-1");
            totalPages.InnerHtml.AppendHtml($"Total Pages: {XPaginationDto.TotalPages.ToString("N0")} ");

            infoDiv.InnerHtml.AppendHtml(totalPages);

            //create a tag that has the total records that was returned by the api
            var totalRecords = new TagBuilder("span");
            totalRecords.AddCssClass("badge badge-primary");
            totalRecords.InnerHtml.AppendHtml($"Total Records: {XPaginationDto.TotalCount.ToString("N0")} ");

            infoDiv.InnerHtml.AppendHtml(totalRecords);

            return infoDiv;
        }
    }
}
