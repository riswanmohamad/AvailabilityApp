using System.ComponentModel.DataAnnotations;

namespace AvailabilityApp.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class Service
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Duration { get; set; }
        public string DurationUnit { get; set; } = "minutes"; // minutes, hours, days, months
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AvailabilityPattern
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string SlotType { get; set; } = string.Empty; // Minute, Hour, Day, Week, Month
        public int SlotDuration { get; set; } // Duration of each slot in minutes
        public TimeSpan? StartTime { get; set; } // For time-based slots
        public TimeSpan? EndTime { get; set; } // For time-based slots
        public string? DaysOfWeek { get; set; } // Comma-separated: '1,2,3,4,5' for Mon-Fri
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // NULL means no end date (ongoing)
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AvailableSlot
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid PatternId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string SlotType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ServiceException
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string ExceptionType { get; set; } = string.Empty; // Unavailable, Holiday, Break, Maintenance
        public bool RecurringYearly { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class SharableLink
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ServiceImage
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string BlobName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}