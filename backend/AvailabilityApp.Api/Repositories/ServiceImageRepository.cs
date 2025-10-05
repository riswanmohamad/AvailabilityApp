using Dapper;
using AvailabilityApp.Api.Models;
using System.Data;

namespace AvailabilityApp.Api.Repositories
{
    public class ServiceImageRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ServiceImageRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<ServiceImage> CreateAsync(ServiceImage serviceImage)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO ServiceImages (Id, ServiceId, BlobName, OriginalFileName, ContentType, FileSize, DisplayOrder, CreatedAt, IsActive)
                VALUES (@Id, @ServiceId, @BlobName, @OriginalFileName, @ContentType, @FileSize, @DisplayOrder, @CreatedAt, @IsActive);";

            serviceImage.Id = Guid.NewGuid();
            serviceImage.CreatedAt = DateTime.UtcNow;
            serviceImage.IsActive = true;

            await connection.ExecuteAsync(sql, serviceImage);
            return serviceImage;
        }

        public async Task<IEnumerable<ServiceImage>> GetByServiceIdAsync(Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, BlobName, OriginalFileName, ContentType, FileSize, DisplayOrder, CreatedAt, IsActive
                FROM ServiceImages 
                WHERE ServiceId = @ServiceId AND IsActive = 1
                ORDER BY DisplayOrder, CreatedAt";

            return await connection.QueryAsync<ServiceImage>(sql, new { ServiceId = serviceId });
        }

        public async Task<ServiceImage?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, BlobName, OriginalFileName, ContentType, FileSize, DisplayOrder, CreatedAt, IsActive
                FROM ServiceImages 
                WHERE Id = @Id AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<ServiceImage>(sql, new { Id = id });
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE ServiceImages 
                SET IsActive = 0 
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> UpdateDisplayOrderAsync(Guid id, int displayOrder)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE ServiceImages 
                SET DisplayOrder = @DisplayOrder 
                WHERE Id = @Id AND IsActive = 1";

            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id, DisplayOrder = displayOrder });
            return affectedRows > 0;
        }

        public async Task<int> GetNextDisplayOrderAsync(Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT ISNULL(MAX(DisplayOrder), 0) + 1
                FROM ServiceImages 
                WHERE ServiceId = @ServiceId AND IsActive = 1";

            return await connection.QuerySingleAsync<int>(sql, new { ServiceId = serviceId });
        }

        public async Task<bool> ExistsAsync(Guid serviceId, string blobName)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT COUNT(1)
                FROM ServiceImages 
                WHERE ServiceId = @ServiceId AND BlobName = @BlobName AND IsActive = 1";

            var count = await connection.QuerySingleAsync<int>(sql, new { ServiceId = serviceId, BlobName = blobName });
            return count > 0;
        }
    }
}