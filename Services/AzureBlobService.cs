using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KumanoKodo.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "kumano-assets";
        private readonly string _blobServiceUrl;
        private readonly string _sasToken;

        private static readonly Dictionary<string, string> MimeTypes = new()
        {
            // Images
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".webp", "image/webp" },
            { ".svg", "image/svg+xml" },
            
            // Audio
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".ogg", "audio/ogg" },
            { ".m4a", "audio/mp4" },
            
            // Video
            { ".mp4", "video/mp4" },
            { ".webm", "video/webm" },
            { ".mov", "video/quicktime" },
            
            // Documents
            { ".pdf", "application/pdf" },
            { ".txt", "text/plain" },
            { ".md", "text/markdown" }
        };

        public AzureBlobService(IConfiguration configuration)
        {
            _blobServiceUrl = configuration["AzureStorage:BlobServiceUrl"] 
                ?? throw new ArgumentNullException(nameof(configuration), "AzureStorage:BlobServiceUrl is not configured");
            
            _sasToken = configuration["AzureStorage:SasToken"] 
                ?? throw new ArgumentNullException(nameof(configuration), "AzureStorage:SasToken is not configured");

            var serviceUrl = new Uri($"{_blobServiceUrl}{_sasToken}");
            _blobServiceClient = new BlobServiceClient(serviceUrl);
        }

        // Lesson Media Methods
        public string GetLessonImageUrl(int lessonId) => GetFileUrl($"lesson_{lessonId}_image.jpg");
        public string GetLessonAudioUrl(int lessonId) => GetFileUrl($"lesson_{lessonId}_audio.mp3");
        
        public async Task<string> UploadLessonImageAsync(int lessonId, Stream content)
            => await UploadFileAsync($"lesson_{lessonId}_image.jpg", content);
        
        public async Task<string> UploadLessonAudioAsync(int lessonId, Stream content)
            => await UploadFileAsync($"lesson_{lessonId}_audio.mp3", content);

        // Vocabulary Media Methods
        public string GetVocabularyImageUrl(int wordId) => GetFileUrl($"vocabulary_{wordId}_image.jpg");
        public string GetVocabularyAudioUrl(int wordId) => GetFileUrl($"vocabulary_{wordId}_audio.mp3");
        
        public async Task<string> UploadVocabularyImageAsync(int wordId, Stream content)
            => await UploadFileAsync($"vocabulary_{wordId}_image.jpg", content);
        
        public async Task<string> UploadVocabularyAudioAsync(int wordId, Stream content)
            => await UploadFileAsync($"vocabulary_{wordId}_audio.mp3", content);

        // Quiz Media Methods
        public string GetQuizImageUrl(int quizId) => GetFileUrl($"quiz_{quizId}_image.jpg");
        public string GetQuizAudioUrl(int quizId) => GetFileUrl($"quiz_{quizId}_audio.mp3");
        
        public async Task<string> UploadQuizImageAsync(int quizId, Stream content)
            => await UploadFileAsync($"quiz_{quizId}_image.jpg", content);
        
        public async Task<string> UploadQuizAudioAsync(int quizId, Stream content)
            => await UploadFileAsync($"quiz_{quizId}_audio.mp3", content);

        // Progress Visualization Methods
        public string GetProgressImageUrl(int userId) => GetFileUrl($"progress_{userId}_map.jpg");
        
        public async Task<string> UploadProgressImageAsync(int userId, Stream content)
            => await UploadFileAsync($"progress_{userId}_map.jpg", content);

        // Generic Methods
        private async Task<string> UploadFileAsync(string fileName, Stream content)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = GetMimeType(fileName) };

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

        private string GetFileUrl(string fileName)
        {
            // Construct URL without exposing the SAS token
            return $"{_blobServiceUrl}/{_containerName}/{fileName}{_sasToken}";
        }

        private static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return MimeTypes.TryGetValue(extension, out var mimeType) 
                ? mimeType 
                : "application/octet-stream";
        }

        // Helper method to get supported file extensions for a given media type
        public static IEnumerable<string> GetSupportedExtensions(string mediaType)
        {
            return MimeTypes
                .Where(x => x.Value == mediaType)
                .Select(x => x.Key);
        }
    }
} 