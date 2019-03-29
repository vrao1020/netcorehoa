namespace HoaWebApplication.Models.PaginationModels
{
    public class PageParametersDto
    {
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string Filters { get; set; }
        public string Sorts { get; set; }
    }
}
