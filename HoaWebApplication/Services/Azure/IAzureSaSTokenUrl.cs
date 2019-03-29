namespace HoaWebApplication.Services.Azure
{
    public interface IAzureSASTokenUrl
    {
        /// <summary>
        /// Returns a url to Azure blob storage that has a SAS token appended to it. 
        /// The SAS token also has content-disposition header set to force file download.
        /// </summary>
        /// <returns></returns>
        string GetDownloadUrlWithSAS(string fileName, string blobStorageFolder);
        /// <summary>
        /// Returns a url to Azure blob storage that has a SAS token appended to it. 
        /// This url will strictly be used for accessing images
        /// </summary>
        /// <returns></returns>
        string GetImageUrlWithSAS(string fileName, string blobStorageFolder);
    }
}
