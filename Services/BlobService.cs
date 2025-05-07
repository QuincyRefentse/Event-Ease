using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using EventEase.Models;

namespace BlobStorage.Services
{
    public class BlobService
    {
        private readonly AzureBlobStorage _settings;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(IOptions<AzureBlobStorage> options)
        {
            _settings = options.Value;

            // Check if the connection string is provided (you don't need SAS token now)
            if (string.IsNullOrEmpty(_settings.ConnectionString))
            {
                throw new ArgumentNullException("ConnectionString", "The connection string is not set in the configuration.");
            }

            // Use the connection string to initialize the BlobServiceClient
            _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        }

        // Uploads a file to the specified container and returns the blob's URI
        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is invalid");

            // Get the BlobContainerClient directly from the _blobServiceClient
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync();

            // Get a reference to the blob (file name will be the blob name)
            var blobClient = containerClient.GetBlobClient(file.FileName);

            // Upload the file stream
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            // Return the URI of the uploaded blob
            return blobClient.Uri.ToString();
        }


        /*
        internal async Task<string> UploadFileAsync(IFormFile imageFile)
        {
            throw new NotImplementedException();
        }
        */
    }
}
