using AvailabilityApp.Api.Models;
using Dapper;

namespace AvailabilityApp.Api.Repositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetByUserIdAsync(Guid userId);
        Task<Service?> GetByIdAsync(Guid id);
        Task<Service?> GetByIdAndUserIdAsync(Guid id, Guid userId);
        Task<Service?> GetByTokenAsync(string token);
        Task<Guid> CreateAsync(Service service);
        Task<bool> UpdateAsync(Service service);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }

    public class ServiceRepository : IServiceRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ServiceRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Service>> GetByUserIdAsync(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, UserId, Title, Description, Duration, 
                       CreatedAt, UpdatedAt, IsActive
                FROM Services 
                WHERE UserId = @UserId AND IsActive = 1
                ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<Service>(sql, new { UserId = userId });
        }

        public async Task<Service?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, UserId, Title, Description, Duration, 
                       CreatedAt, UpdatedAt, IsActive
                FROM Services 
                WHERE Id = @Id AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<Service>(sql, new { Id = id });
        }

        public async Task<Service?> GetByIdAndUserIdAsync(Guid id, Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, UserId, Title, Description, Duration, 
                       CreatedAt, UpdatedAt, IsActive
                FROM Services 
                WHERE Id = @Id AND UserId = @UserId AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<Service>(sql, new { Id = id, UserId = userId });
        }

        public async Task<Service?> GetByTokenAsync(string token)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT s.Id, s.UserId, s.Title, s.Description, s.Duration, 
                       s.CreatedAt, s.UpdatedAt, s.IsActive
                FROM Services s
                INNER JOIN SharableLinks sl ON s.Id = sl.ServiceId
                WHERE sl.Token = @Token AND s.IsActive = 1 AND sl.IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<Service>(sql, new { Token = token });
        }

        public async Task<Guid> CreateAsync(Service service)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO Services (Id, UserId, Title, Description, Duration, CreatedAt, UpdatedAt, IsActive)
                VALUES (@Id, @UserId, @Title, @Description, @Duration, @CreatedAt, @UpdatedAt, @IsActive);
                SELECT @Id;";

            service.Id = Guid.NewGuid();
            service.CreatedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;
            service.IsActive = true;

            await connection.ExecuteAsync(sql, service);
            return service.Id;
        }

        public async Task<bool> UpdateAsync(Service service)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Services 
                SET Title = @Title, Description = @Description, Duration = @Duration, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND UserId = @UserId";

            service.UpdatedAt = DateTime.UtcNow;
            var affectedRows = await connection.ExecuteAsync(sql, service);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Services 
                SET IsActive = 0, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND UserId = @UserId";

            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id, UserId = userId, UpdatedAt = DateTime.UtcNow });
            return affectedRows > 0;
        }
    }
}