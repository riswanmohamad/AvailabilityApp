using AvailabilityApp.Api.Models;
using Dapper;

namespace AvailabilityApp.Api.Repositories
{
    public interface IAvailabilityRepository
    {
        Task<IEnumerable<AvailabilityPattern>> GetPatternsByServiceIdAsync(Guid serviceId);
        Task<AvailabilityPattern?> GetPatternByIdAsync(Guid id);
        Task<Guid> CreatePatternAsync(AvailabilityPattern pattern);
        Task<bool> UpdatePatternAsync(AvailabilityPattern pattern);
        Task<bool> DeletePatternAsync(Guid id, Guid serviceId);
        Task<IEnumerable<AvailableSlot>> GetSlotsByServiceIdAsync(Guid serviceId, DateTime startDate, DateTime endDate);
        Task<bool> CreateSlotsAsync(IEnumerable<AvailableSlot> slots);
        Task<bool> DeleteSlotsByPatternIdAsync(Guid patternId);
    }

    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AvailabilityRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<AvailabilityPattern>> GetPatternsByServiceIdAsync(Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, SlotType, SlotDuration, StartTime, EndTime, DaysOfWeek, 
                       StartDate, EndDate, CreatedAt, IsActive
                FROM AvailabilityPatterns 
                WHERE ServiceId = @ServiceId AND IsActive = 1
                ORDER BY CreatedAt";

            return await connection.QueryAsync<AvailabilityPattern>(sql, new { ServiceId = serviceId });
        }

        public async Task<AvailabilityPattern?> GetPatternByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, SlotType, SlotDuration, StartTime, EndTime, DaysOfWeek, 
                       StartDate, EndDate, CreatedAt, IsActive
                FROM AvailabilityPatterns 
                WHERE Id = @Id AND IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<AvailabilityPattern>(sql, new { Id = id });
        }

        public async Task<Guid> CreatePatternAsync(AvailabilityPattern pattern)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO AvailabilityPatterns (Id, ServiceId, SlotType, SlotDuration, StartTime, EndTime, 
                                                DaysOfWeek, StartDate, EndDate, CreatedAt, IsActive)
                VALUES (@Id, @ServiceId, @SlotType, @SlotDuration, @StartTime, @EndTime, 
                        @DaysOfWeek, @StartDate, @EndDate, @CreatedAt, @IsActive);
                SELECT @Id;";

            pattern.Id = Guid.NewGuid();
            pattern.CreatedAt = DateTime.UtcNow;
            pattern.IsActive = true;

            await connection.ExecuteAsync(sql, pattern);
            return pattern.Id;
        }

        public async Task<bool> UpdatePatternAsync(AvailabilityPattern pattern)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE AvailabilityPatterns 
                SET SlotType = @SlotType, SlotDuration = @SlotDuration, StartTime = @StartTime, 
                    EndTime = @EndTime, DaysOfWeek = @DaysOfWeek, StartDate = @StartDate, 
                    EndDate = @EndDate
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, pattern);
            return affectedRows > 0;
        }

        public async Task<bool> DeletePatternAsync(Guid id, Guid serviceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE AvailabilityPatterns 
                SET IsActive = 0
                WHERE Id = @Id AND ServiceId = @ServiceId";

            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id, ServiceId = serviceId });
            return affectedRows > 0;
        }

        public async Task<IEnumerable<AvailableSlot>> GetSlotsByServiceIdAsync(Guid serviceId, DateTime startDate, DateTime endDate)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, ServiceId, PatternId, StartDateTime, EndDateTime, SlotType, IsAvailable, CreatedAt
                FROM AvailableSlots 
                WHERE ServiceId = @ServiceId 
                  AND StartDateTime >= @StartDate 
                  AND StartDateTime <= @EndDate
                ORDER BY StartDateTime";

            return await connection.QueryAsync<AvailableSlot>(sql, new 
            { 
                ServiceId = serviceId, 
                StartDate = startDate, 
                EndDate = endDate 
            });
        }

        public async Task<bool> CreateSlotsAsync(IEnumerable<AvailableSlot> slots)
        {
            if (!slots.Any()) return true;

            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO AvailableSlots (Id, ServiceId, PatternId, StartDateTime, EndDateTime, SlotType, IsAvailable, CreatedAt)
                VALUES (@Id, @ServiceId, @PatternId, @StartDateTime, @EndDateTime, @SlotType, @IsAvailable, @CreatedAt)";

            var affectedRows = await connection.ExecuteAsync(sql, slots);
            return affectedRows == slots.Count();
        }

        public async Task<bool> DeleteSlotsByPatternIdAsync(Guid patternId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "DELETE FROM AvailableSlots WHERE PatternId = @PatternId";

            await connection.ExecuteAsync(sql, new { PatternId = patternId });
            return true;
        }
    }
}