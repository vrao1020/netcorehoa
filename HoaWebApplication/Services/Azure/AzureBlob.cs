using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HoaWebApplication.Services.Azure
{
    public class AzureBlob : IAzureBlob
    {
        private IConfiguration _configuration;
        private IAzureBlobContainer _azureBlobContainer;
        private ILogger<AzureBlob> _logger;

        public AzureBlob(IConfiguration configuration, IAzureBlobContainer azureBlobContainer, ILogger<AzureBlob> logger)
        {
            _configuration = configuration;
            _azureBlobContainer = azureBlobContainer;
            _logger = logger;
        }

        //public CloudBlobContainer GetAzureBlobContainer()
        //{
        //    CloudStorageAccount storageAccount = null;

        //    if (CloudStorageAccount.TryParse(_configuration["ConnectionStrings:AzureStorageConnectionString:StorageContainer"], out storageAccount))
        //    {
        //        // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
        //        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //        CloudBlobContainer container = blobClient.GetContainerReference(_configuration["ConnectionStrings:AzureStorageConnectionString:BlobContainer"]);

        //        return container;
        //    }

        //    else
        //    {
        //        //might want to build around making the website available even when azure is down
        //        //right now the website will always redirect user to an error page when an azure container cannot be retrived
        //        throw new Exception("Azure is unavailable at this time, please try again in a few minutes");
        //    }
        //}

        public CloudBlockBlob GetAzureBlobFileReference(string fileName, string blobStorageFolder)
        {
            //get a reference to the blob directory where we will get a reference for the file
            var blobDirectory = _azureBlobContainer.GetAzureBlockBlobContainer()
                                                   .GetDirectoryReference(blobStorageFolder);

            //get a reference to the blob file so that it can be downloaded/uploaded/checked 
            //to see if it exists            
            CloudBlockBlob cloudBlockBlob = blobDirectory.GetBlockBlobReference(fileName);

            //CloudBlockBlob cloudBlockBlob = GetAzureBlobContainer().GetBlockBlobReference(fileName);

            return cloudBlockBlob;
        }

        public async Task<IEnumerable<string>> GetAzureBlobAllDirectoryFileReferencesAsync(string blobStorageFolder)
        {
            List<string> BlobFiles = new List<string>();

            // List the blobs in the container.
            //continuation token is not required as ListBlobsSegmentedAsync will return upto
            //5000 files. Past 5000 files, the continuation token will start being used
            BlobContinuationToken blobContinuationToken = null;

            do
            {
                var blobDirectory = _azureBlobContainer.GetAzureBlockBlobContainer()
                                                       .GetDirectoryReference(blobStorageFolder);

                //var results = await _azureBlobContainer.GetAzureBlockBlobContainer().ListBlobsSegmentedAsync(null, blobContinuationToken);
                var results = await blobDirectory.ListBlobsSegmentedAsync(blobContinuationToken);

                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;

                foreach (IListBlobItem item in results.Results)
                {
                    //need to extract file name from the absolute path
                    //by default the properties does not include a file name so have to take below approach        
                    BlobFiles.Add(System.IO.Path.GetFileName(item.Uri.AbsolutePath));
                }

            } while (blobContinuationToken != null); // Loop while the continuation token is not null.

            //return the list of files from Azure blob storage
            return BlobFiles;
        }

        public async Task<bool> AzureBlobFileExistsAsync(string fileName, string blobStorageFolder)
        {
            //get a reference to the blob file and check if it exists
            return await GetAzureBlobFileReference(fileName, blobStorageFolder).ExistsAsync();
        }

        public async Task UploadAzureBlobFileAsync(IFormFile fileToUpload, CloudBlockBlob cloudBlockBlob)
        {
            using (var stream = fileToUpload.OpenReadStream())
            {
                try
                {
                    await cloudBlockBlob.UploadFromStreamAsync(stream);
                }
                catch (StorageException ex)
                {
                    _logger.LogError($"An error occured while uploading the file." +
                                 $" Error Message: {ex.Message}");

                    throw new Exception("File upload failed. Please try again later");
                }
            }
        }

        public async Task UploadAzureBlobFileAsync(Stream stream, CloudBlockBlob cloudBlockBlob)
        {
            try
            {
                await cloudBlockBlob.UploadFromStreamAsync(stream);
            }
            catch (StorageException ex)
            {
                _logger.LogError($"An error occured while uploading the file." +
                                 $" Error Message: {ex.Message}");

                throw new Exception("File upload failed. Please try again later");
            }

        }

        public async Task DeleteAzureBlobFileAsync(CloudBlockBlob cloudBlockBlob)
        {
            try
            {
                await cloudBlockBlob.DeleteAsync();
            }
            catch (StorageException ex)
            {
                _logger.LogError($"An error occured while deleting the file." +
                                 $" Error Message: {ex.Message}");

                throw new Exception("File delete failed. Please try again later");
            }
        }
    }
}
