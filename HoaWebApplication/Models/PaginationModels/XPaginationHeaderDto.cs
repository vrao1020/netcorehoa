namespace HoaWebApplication.Models.PaginationModels
{
    //This class is deserialized from the X-Pagination header when calling the API
    //It will be used for showing paginated data on the website
    public class XPaginationHeaderDto
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string PreviousPageLink { get; set; }
        public string NextPageLink { get; set; }
    }
}
