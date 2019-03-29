using Microsoft.WindowsAzure.Storage.Blob;

namespace HoaWebApplication.Services.Azure
{
    public interface IAzureBlobContainer
    {
        CloudBlobContainer GetAzureBlockBlobContainer();
    }
}
