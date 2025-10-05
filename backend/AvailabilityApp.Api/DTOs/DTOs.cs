using System.ComponentModel.DataAnnotations;

namespace AvailabilityApp.Api.DTOs
{
    // Authentication DTOs
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? BusinessName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    // Service DTOs
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? SharableToken { get; set; }
        public List<ServiceImageDto> Images { get; set; } = new();
    }

    public class CreateServiceDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Duration { get; set; }
    }

    public class UpdateServiceDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Duration { get; set; }
    }

    // Availability DTOs
    public class AvailabilityPatternDto
    {
        public Guid Id { get; set; }
        public string SlotType { get; set; } = string.Empty;
        public int SlotDuration { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? DaysOfWeek { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateAvailabilityPatternDto
    {
        [Required]
        public string SlotType { get; set; } = string.Empty; // Minute, Hour, Day, Week, Month

        [Required]
        [Range(1, int.MaxValue)]
        public int SlotDuration { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? DaysOfWeek { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AvailableSlotDto
    {
        public Guid Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string SlotType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    // Exception DTOs
    public class ExceptionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public bool RecurringYearly { get; set; }
    }

    public class CreateExceptionDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Required]
        public string ExceptionType { get; set; } = string.Empty;
        public bool RecurringYearly { get; set; }
    }

    // Public DTOs (for sharable links)
    public class PublicServiceDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Duration { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
        public List<ServiceImageDto> Images { get; set; } = new();
    }

    // Response DTOs
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class SharableLinkDto
    {
        public string Token { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Image DTOs
    public class ServiceImageDto
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}