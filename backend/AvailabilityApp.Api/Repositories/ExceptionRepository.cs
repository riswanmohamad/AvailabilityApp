using AvailabilityApp.Api.Models;
using Dapper;

namespace AvailabilityApp.Api.Repositories
{
    public interface IExceptionRepository
    {
        Task<IEnumerable<ServiceException>> GetByServiceIdAsync(Guid serviceId);
        Task<ServiceException?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(ServiceException exception);
        Task<bool> UpdateAsync(ServiceException exception);
        Task<bool> DeleteAsync(Guid id, Guid serviceId);
        Task<IEnumerable<ServiceException>> GetActiveExceptionsForPeriodAsync(Guid serviceId, DateTime startDate, DateTime endDate);
    }

    public class ExceptionRepository : IExceptionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ExceptionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ServiceException>> GetByServiceIdAsync(Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, Title, Description, StartDateTime, EndDateTime, 
                       ExceptionType, RecurringYearly, CreatedAt, IsActive
                FROM Exceptions 
                WHERE ServiceId = @ServiceId AND IsActive = 1
                ORDER BY StartDateTime";

            return await connection.QueryAsync<ServiceException>(sql, new { ServiceId = serviceId });
        }

        public async Task<ServiceException?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, Title, Description, StartDateTime, EndDateTime, 
                       ExceptionType, RecurringYearly, CreatedAt, IsActive
                FROM Exceptions 
                WHERE Id = @Id AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<ServiceException>(sql, new { Id = id });
        }

        public async Task<Guid> CreateAsync(ServiceException exception)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO Exceptions (Id, ServiceId, Title, Description, StartDateTime, EndDateTime, 
                                      ExceptionType, RecurringYearly, CreatedAt, IsActive)
                VALUES (@Id, @ServiceId, @Title, @Description, @StartDateTime, @EndDateTime, 
                        @ExceptionType, @RecurringYearly, @CreatedAt, @IsActive);
                SELECT @Id;";

            exception.Id = Guid.NewGuid();
            exception.CreatedAt = DateTime.UtcNow;
            exception.IsActive = true;

            await connection.ExecuteAsync(sql, exception);
            return exception.Id;
        }

        public async Task<bool> UpdateAsync(ServiceException exception)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Exceptions 
                SET Title = @Title, Description = @Description, StartDateTime = @StartDateTime, 
                    EndDateTime = @EndDateTime, ExceptionType = @ExceptionType, RecurringYearly = @RecurringYearly
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, exception);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE Exceptions 
                SET IsActive = 0
                WHERE Id = @Id AND ServiceId = @ServiceId";

            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id, ServiceId = serviceId });
            return affectedRows > 0;
        }

        public async Task<IEnumerable<ServiceException>> GetActiveExceptionsForPeriodAsync(Guid serviceId, DateTime startDate, DateTime endDate)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, Title, Description, StartDateTime, EndDateTime, 
                       ExceptionType, RecurringYearly, CreatedAt, IsActive
                FROM Exceptions 
                WHERE ServiceId = @ServiceId AND IsActive = 1
                  AND ((StartDateTime <= @EndDate AND EndDateTime >= @StartDate)
                       OR (RecurringYearly = 1 AND 
                           ((MONTH(StartDateTime) < MONTH(EndDateTime) AND 
                             ((MONTH(@StartDate) >= MONTH(StartDateTime) AND MONTH(@StartDate) <= MONTH(EndDateTime)) OR
                              (MONTH(@EndDate) >= MONTH(StartDateTime) AND MONTH(@EndDate) <= MONTH(EndDateTime))))
                            OR (MONTH(StartDateTime) > MONTH(EndDateTime) AND 
                                ((MONTH(@StartDate) >= MONTH(StartDateTime) OR MONTH(@StartDate) <= MONTH(EndDateTime)) OR
                                 (MONTH(@EndDate) >= MONTH(StartDateTime) OR MONTH(@EndDate) <= MONTH(EndDateTime)))))))";

            return await connection.QueryAsync<ServiceException>(sql, new 
            { 
                ServiceId = serviceId, 
                StartDate = startDate, 
                EndDate = endDate 
            });
        }
    }
}