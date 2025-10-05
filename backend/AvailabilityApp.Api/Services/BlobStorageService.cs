using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AvailabilityApp.Api.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(Stream stream, string fileName, string contentType);
        Task<bool> DeleteAsync(string blobName);
        Task<bool> ExistsAsync(string blobName);
        Task<Stream> DownloadAsync(string blobName);
        string GetBlobUrl(string blobName);
        string GenerateUniqueBlobName(string originalFileName);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "service-images";
            
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
        {
            // Ensure container exists
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobName = GenerateUniqueBlobName(fileName);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Conditions = null
            });

            return blobName;
        }

        public async Task<bool> DeleteAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Stream> DownloadAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }

        public string GetBlobUrl(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return blobClient.Uri.ToString();
        }

        public string GenerateUniqueBlobName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID
            
            return $"{fileName}_{timestamp}_{uniqueId}{extension}".ToLowerInvariant();
        }
    }
}