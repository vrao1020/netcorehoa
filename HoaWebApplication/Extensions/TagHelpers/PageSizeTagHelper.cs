using HoaWebApplication.Models.PaginationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HoaWebApplication.Extensions.TagHelpers
{
    public class PageSizeTagHelper : TagHelper
    {
        public XPaginationHeaderDto XPaginationDto { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (XPaginationDto.TotalPages >= 1)
            {
                output.Content.AppendHtml(CreatePageSizeControl());
            }
        }

        private TagBuilder CreatePageSizeControl()
        {
            //create a select tag
            var dropDown = new TagBuilder("select");
            dropDown.AddCssClass($"page-size-control");

            for (int i = 1; i <= 3; i++)
            {
                //loop through and add option tag
                var option = new TagBuilder("option");

                //set the value of the option to be 5/10/15/etc
                option.InnerHtml.AppendHtml($"{i * 5}");

                //if current selected page size equals i * 5 then make that option tag selected
                if ((i * 5) == XPaginationDto.PageSize)
                {
                    option.Attributes.Add("selected", "selected");
                }

                //add all the option tag helpers to the select tag
                dropDown.InnerHtml.AppendHtml(option);
            }

            //create a div tag
            var fGroup = new TagBuilder("div");
            fGroup.AddCssClass("form-group badge ml-0 pl-0");

            //create label tag
            var label = new TagBuilder("label");
            label.InnerHtml.AppendHtml($"Items Per Page&nbsp;");
            fGroup.InnerHtml.AppendHtml(label);
            fGroup.InnerHtml.AppendHtml(dropDown);

            return fGroup;
        }
    }
}
