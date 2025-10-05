using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Repositories;

namespace AvailabilityApp.Api.Services
{
    public interface IPublicService
    {
        Task<ApiResponse<PublicServiceDto>> GetPublicServiceAsync(string token);
    }

    public class PublicService : IPublicService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IExceptionRepository _exceptionRepository;
        private readonly ISharableLinkRepository _sharableLinkRepository;
        private readonly ServiceImageRepository _serviceImageRepository;
        private readonly IBlobStorageService _blobStorageService;

        public PublicService(
            IServiceRepository serviceRepository,
            IUserRepository userRepository,
            IAvailabilityRepository availabilityRepository,
            IExceptionRepository exceptionRepository,
            ISharableLinkRepository sharableLinkRepository,
            ServiceImageRepository serviceImageRepository,
            IBlobStorageService blobStorageService)
        {
            _serviceRepository = serviceRepository;
            _userRepository = userRepository;
            _availabilityRepository = availabilityRepository;
            _exceptionRepository = exceptionRepository;
            _sharableLinkRepository = sharableLinkRepository;
            _serviceImageRepository = serviceImageRepository;
            _blobStorageService = blobStorageService;
        }

        public async Task<ApiResponse<PublicServiceDto>> GetPublicServiceAsync(string token)
        {
            try
            {
                var sharableLink = await _sharableLinkRepository.GetByTokenAsync(token);
                if (sharableLink == null)
                {
                    return new ApiResponse<PublicServiceDto>
                    {
                        Success = false,
                        Message = "Service not found or link is invalid"
                    };
                }

                var service = await _serviceRepository.GetByIdAsync(sharableLink.ServiceId);
                if (service == null)
                {
                    return new ApiResponse<PublicServiceDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var user = await _userRepository.GetByIdAsync(service.UserId);
                if (user == null)
                {
                    return new ApiResponse<PublicServiceDto>
                    {
                        Success = false,
                        Message = "Service provider not found"
                    };
                }

                // Get available slots for the next 30 days
                var startDate = DateTime.UtcNow.Date;
                var endDate = startDate.AddDays(30);

                var slots = await _availabilityRepository.GetSlotsByServiceIdAsync(service.Id, startDate, endDate);
                var exceptions = await _exceptionRepository.GetActiveExceptionsForPeriodAsync(service.Id, startDate, endDate);

                // Filter slots by exceptions
                var availableSlots = FilterSlotsByExceptions(slots, exceptions);

                var slotDtos = availableSlots.Where(s => s.IsAvailable).Select(s => new AvailableSlotDto
                {
                    Id = s.Id,
                    StartDateTime = s.StartDateTime,
                    EndDateTime = s.EndDateTime,
                    SlotType = s.SlotType,
                    IsAvailable = s.IsAvailable
                }).OrderBy(s => s.StartDateTime);

                // Get service images
                var images = await _serviceImageRepository.GetByServiceIdAsync(service.Id);
                var imageDtos = images.Select(img => new ServiceImageDto
                {
                    Id = img.Id,
                    ServiceId = img.ServiceId,
                    Url = _blobStorageService.GetBlobUrl(img.BlobName),
                    OriginalFileName = img.OriginalFileName,
                    ContentType = img.ContentType,
                    FileSize = img.FileSize,
                    DisplayOrder = img.DisplayOrder,
                    CreatedAt = img.CreatedAt
                }).ToList();

                var publicServiceDto = new PublicServiceDto
                {
                    Title = service.Title,
                    Description = service.Description,
                    Duration = service.Duration,
                    ProviderName = $"{user.FirstName} {user.LastName}",
                    BusinessName = user.BusinessName,
                    AvailableSlots = slotDtos.ToList(),
                    Images = imageDtos
                };

                return new ApiResponse<PublicServiceDto>
                {
                    Success = true,
                    Data = publicServiceDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PublicServiceDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching service details",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private IEnumerable<AvailabilityApp.Api.Models.AvailableSlot> FilterSlotsByExceptions(
            IEnumerable<AvailabilityApp.Api.Models.AvailableSlot> slots, 
            IEnumerable<AvailabilityApp.Api.Models.ServiceException> exceptions)
        {
            var filteredSlots = new List<AvailabilityApp.Api.Models.AvailableSlot>();

            foreach (var slot in slots)
            {
                var isAvailable = true;

                foreach (var exception in exceptions)
                {
                    if (exception.RecurringYearly)
                    {
                        // Check yearly recurring exceptions
                        var exceptionStart = new DateTime(slot.StartDateTime.Year, exception.StartDateTime.Month, exception.StartDateTime.Day);
                        var exceptionEnd = new DateTime(slot.StartDateTime.Year, exception.EndDateTime.Month, exception.EndDateTime.Day);

                        if (slot.StartDateTime >= exceptionStart && slot.StartDateTime <= exceptionEnd)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                    else
                    {
                        // Check one-time exceptions
                        if (slot.StartDateTime >= exception.StartDateTime && slot.StartDateTime <= exception.EndDateTime)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                }

                slot.IsAvailable = isAvailable;
                filteredSlots.Add(slot);
            }

            return filteredSlots;
        }
    }
}