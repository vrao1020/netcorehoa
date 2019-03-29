namespace HoaWebApplication.Models.CacheModel
{
    public class CacheDto
    {
        public string Etag { get; set; }
        public string ResponseContent { get; set; }
        public string XPagination { get; set; }

        public CacheDto(string etag, string responseContent, string xPagination)
        {
            Etag = etag;
            ResponseContent = responseContent;
            XPagination = xPagination;
        }
    }
}
