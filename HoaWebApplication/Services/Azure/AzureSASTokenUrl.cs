using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace HoaWebApplication.Services.Azure
{
    public class AzureSASTokenUrl : IAzureSASTokenUrl
    {
        private IAzureBlob _getAzureFile;

        public AzureSASTokenUrl(IAzureBlob getAzureFile)
        {
            _getAzureFile = getAzureFile;
        }

        public string GetDownloadUrlWithSAS(string fileName, string blobStorageFolder)
        {
            var blobReference = _getAzureFile.GetAzureBlobFileReference(fileName, blobStorageFolder);
            var name = blobReference.Name;

            //Set the expiry time and permissions for the blob.
            //In this case, the start time is specified as a minute in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-1),
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(40)
            };

            //Set content-disposition header as this will force the URL to be downloaded instead of opening up the link in a page
            SharedAccessBlobHeaders headers = new SharedAccessBlobHeaders()
            {
                ContentDisposition = $"attachment;filename=\"{name}\""
            };

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blobReference.GetSharedAccessSignature(sasConstraints, headers);

            //Return the URI string for the container, including the SAS token.
            var uri = blobReference.Uri.AbsoluteUri + sasBlobToken;

            return uri;
        }

        public string GetImageUrlWithSAS(string fileName, string blobStorageFolder)
        {
            var blobReference = _getAzureFile.GetAzureBlobFileReference(fileName, blobStorageFolder);
            var name = blobReference.Name;

            //Set the expiry time and permissions for the blob.
            //In this case, the start time is specified as a minute in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy()
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-1),
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(30)
            };

            //Generate the shared access signature on the blob, setting the constraints directly on the blob.
            string sasBlobToken = blobReference.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            var uri = blobReference.Uri.AbsoluteUri + sasBlobToken;

            return uri;
        }
    }
}
