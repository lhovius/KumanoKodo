using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KumanoKodo.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "kumano-assets";
        private readonly string _blobServiceUrl;
        private readonly string _sasToken;

        public AzureBlobService(IConfiguration configuration)
        {
            _blobServiceUrl = configuration["AzureStorage:BlobServiceUrl"] 
                ?? throw new ArgumentNullException(nameof(configuration), "AzureStorage:BlobServiceUrl is not configured");
            
            _sasToken = configuration["AzureStorage:SasToken"] 
                ?? throw new ArgumentNullException(nameof(configuration), "AzureStorage:SasToken is not configured");

            var serviceUrl = new Uri($"{_blobServiceUrl}{_sasToken}");
            _blobServiceClient = new BlobServiceClient(serviceUrl);
        }

        public async Task<string> UploadFileAsync(string fileName, Stream content, string contentType)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

            await blobClient.UploadAsync(content, blobHttpHeaders);
            return GetFileUrl(fileName);
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetFileUrl(string fileName)
        {
            // Construct URL without exposing the SAS token
            return $"{_blobServiceUrl}/{_containerName}/{fileName}{_sasToken}";
        }
    }
} 