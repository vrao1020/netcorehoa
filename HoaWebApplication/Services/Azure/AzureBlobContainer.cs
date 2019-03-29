using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace HoaWebApplication.Services.Azure
{
    public class AzureBlobContainer : IAzureBlobContainer
    {
        private IConfiguration _configuration;

        public AzureBlobContainer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CloudBlobContainer GetAzureBlockBlobContainer()
        {
            CloudStorageAccount storageAccount = null;

            if (CloudStorageAccount.TryParse(_configuration["ConnectionStrings:AzureStorageConnectionString:StorageContainer"], out storageAccount))
            {
                // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_configuration["ConnectionStrings:AzureStorageConnectionString:BlobContainer"]);

                return container;
            }

            else
            {
                //might want to build around making the website available even when azure is down
                //right now the website will always redirect user to an error page when an azure container cannot be retrived
                throw new Exception("Azure is unavailable at this time, please try again in a few minutes");
            }
        }
    }
}
