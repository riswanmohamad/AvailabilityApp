using AvailabilityApp.Api.Models;
using Dapper;

namespace AvailabilityApp.Api.Repositories
{
    public interface ISharableLinkRepository
    {
        Task<SharableLink?> GetByServiceIdAsync(Guid serviceId);
        Task<SharableLink?> GetByTokenAsync(string token);
        Task<string> CreateAsync(Guid serviceId);
        Task<bool> DeactivateByServiceIdAsync(Guid serviceId);
        Task<string> RegenerateAsync(Guid serviceId);
    }

    public class SharableLinkRepository : ISharableLinkRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SharableLinkRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<SharableLink?> GetByServiceIdAsync(Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, Token, CreatedAt, IsActive
                FROM SharableLinks 
                WHERE ServiceId = @ServiceId AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<SharableLink>(sql, new { ServiceId = serviceId });
        }

        public async Task<SharableLink?> GetByTokenAsync(string token)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, Token, CreatedAt, IsActive
                FROM SharableLinks 
                WHERE Token = @Token AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<SharableLink>(sql, new { Token = token });
        }

        public async Task<string> CreateAsync(Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Deactivate existing links first
            await DeactivateByServiceIdAsync(serviceId);

            // Create new link
            var token = GenerateToken();
            var sql = @"
                INSERT INTO SharableLinks (Id, ServiceId, Token, CreatedAt, IsActive)
                VALUES (@Id, @ServiceId, @Token, @CreatedAt, @IsActive)";

            await connection.ExecuteAsync(sql, new 
            {
                Id = Guid.NewGuid(),
                ServiceId = serviceId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            return token;
        }

        public async Task<bool> DeactivateByServiceIdAsync(Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE SharableLinks 
                SET IsActive = 0
                WHERE ServiceId = @ServiceId";

            await connection.ExecuteAsync(sql, new { ServiceId = serviceId });
            return true;
        }

        public async Task<string> RegenerateAsync(Guid serviceId)
        {
            return await CreateAsync(serviceId);
        }

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString("N"); // 32 character string without hyphens
        }
    }
}