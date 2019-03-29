using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HoaWebApplication.Services.Azure
{
    public interface IAzureBlob
    {
        //CloudBlobContainer GetAzureBlobContainer();
        Task<bool> AzureBlobFileExistsAsync(string fileName, string blobStorageFolder);
        CloudBlockBlob GetAzureBlobFileReference(string fileName, string blobStorageFolder);
        Task<IEnumerable<string>> GetAzureBlobAllDirectoryFileReferencesAsync(string blobStorageFolder);
        Task UploadAzureBlobFileAsync(IFormFile fileToUpload, CloudBlockBlob cloudBlockBlob);
        Task UploadAzureBlobFileAsync(Stream stream, CloudBlockBlob cloudBlockBlob);
        Task DeleteAzureBlobFileAsync(CloudBlockBlob cloudBlockBlob);
    }
}
