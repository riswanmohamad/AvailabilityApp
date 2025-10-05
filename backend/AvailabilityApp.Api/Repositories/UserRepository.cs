using AvailabilityApp.Api.Models;
using Dapper;

namespace AvailabilityApp.Api.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<Guid> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> ExistsAsync(string email);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, Email, PasswordHash, FirstName, LastName, BusinessName, PhoneNumber, 
                       CreatedAt, UpdatedAt, IsActive
                FROM Users 
                WHERE Id = @Id AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, Email, PasswordHash, FirstName, LastName, BusinessName, PhoneNumber, 
                       CreatedAt, UpdatedAt, IsActive
                FROM Users 
                WHERE Email = @Email AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<Guid> CreateAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, BusinessName, PhoneNumber, CreatedAt, UpdatedAt, IsActive)
                VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @BusinessName, @PhoneNumber, @CreatedAt, @UpdatedAt, @IsActive);
                SELECT @Id;";

            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;

            await connection.ExecuteAsync(sql, user);
            return user.Id;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Users 
                SET Email = @Email, FirstName = @FirstName, LastName = @LastName, 
                    BusinessName = @BusinessName, PhoneNumber = @PhoneNumber, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            user.UpdatedAt = DateTime.UtcNow;
            var affectedRows = await connection.ExecuteAsync(sql, user);
            return affectedRows > 0;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND IsActive = 1";
            var count = await connection.QueryFirstOrDefaultAsync<int>(sql, new { Email = email });
            return count > 0;
        }
    }
}