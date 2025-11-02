using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Models;
using AvailabilityApp.Api.Repositories;

namespace AvailabilityApp.Api.Services
{
    public interface IServiceManagementService
    {
        Task<ApiResponse<IEnumerable<ServiceDto>>> GetUserServicesAsync(Guid userId);
        Task<ApiResponse<ServiceDto>> GetServiceAsync(Guid serviceId, Guid userId);
        Task<ApiResponse<ServiceDto>> CreateServiceAsync(CreateServiceDto createServiceDto, Guid userId);
        Task<ApiResponse<ServiceDto>> UpdateServiceAsync(Guid serviceId, UpdateServiceDto updateServiceDto, Guid userId);
        Task<ApiResponse<bool>> DeleteServiceAsync(Guid serviceId, Guid userId);
        Task<ApiResponse<SharableLinkDto>> GenerateSharableLinkAsync(Guid serviceId, Guid userId);
        Task<ApiResponse<SharableLinkDto>> RegenerateSharableLinkAsync(Guid serviceId, Guid userId);
    }

    public class ServiceManagementService : IServiceManagementService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ISharableLinkRepository _sharableLinkRepository;
        private readonly ServiceImageRepository _serviceImageRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;

        public ServiceManagementService(
            IServiceRepository serviceRepository, 
            ISharableLinkRepository sharableLinkRepository,
            ServiceImageRepository serviceImageRepository,
            IBlobStorageService blobStorageService,
            IConfiguration configuration)
        {
            _serviceRepository = serviceRepository;
            _sharableLinkRepository = sharableLinkRepository;
            _serviceImageRepository = serviceImageRepository;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
        }

        public async Task<ApiResponse<IEnumerable<ServiceDto>>> GetUserServicesAsync(Guid userId)
        {
            try
            {
                var services = await _serviceRepository.GetByUserIdAsync(userId);
                var serviceDtos = new List<ServiceDto>();

                foreach (var service in services)
                {
                    var sharableLink = await _sharableLinkRepository.GetByServiceIdAsync(service.Id);
                    var serviceDto = await MapToServiceDtoAsync(service, sharableLink);
                    serviceDtos.Add(serviceDto);
                }

                return new ApiResponse<IEnumerable<ServiceDto>>
                {
                    Success = true,
                    Data = serviceDtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<ServiceDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching services",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ServiceDto>> GetServiceAsync(Guid serviceId, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<ServiceDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var sharableLink = await _sharableLinkRepository.GetByServiceIdAsync(service.Id);
                var serviceDto = await MapToServiceDtoAsync(service, sharableLink);

                return new ApiResponse<ServiceDto>
                {
                    Success = true,
                    Data = serviceDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching service",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ServiceDto>> CreateServiceAsync(CreateServiceDto createServiceDto, Guid userId)
        {
            try
            {
                var service = new Service
                {
                    UserId = userId,
                    Title = createServiceDto.Title,
                    Description = createServiceDto.Description,
                    Duration = createServiceDto.Duration,
                    DurationUnit = createServiceDto.DurationUnit
                };

                var serviceId = await _serviceRepository.CreateAsync(service);
                service.Id = serviceId;

                var serviceDto = await MapToServiceDtoAsync(service);

                return new ApiResponse<ServiceDto>
                {
                    Success = true,
                    Message = "Service created successfully",
                    Data = serviceDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceDto>
                {
                    Success = false,
                    Message = "An error occurred while creating service",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ServiceDto>> UpdateServiceAsync(Guid serviceId, UpdateServiceDto updateServiceDto, Guid userId)
        {
            try
            {
                var existingService = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (existingService == null)
                {
                    return new ApiResponse<ServiceDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                existingService.Title = updateServiceDto.Title;
                existingService.Description = updateServiceDto.Description;
                existingService.Duration = updateServiceDto.Duration;
                existingService.DurationUnit = updateServiceDto.DurationUnit;

                var updated = await _serviceRepository.UpdateAsync(existingService);
                if (!updated)
                {
                    return new ApiResponse<ServiceDto>
                    {
                        Success = false,
                        Message = "Failed to update service"
                    };
                }

                var sharableLink = await _sharableLinkRepository.GetByServiceIdAsync(existingService.Id);
                var serviceDto = await MapToServiceDtoAsync(existingService, sharableLink);

                return new ApiResponse<ServiceDto>
                {
                    Success = true,
                    Message = "Service updated successfully",
                    Data = serviceDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceDto>
                {
                    Success = false,
                    Message = "An error occurred while updating service",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteServiceAsync(Guid serviceId, Guid userId)
        {
            try
            {
                var deleted = await _serviceRepository.DeleteAsync(serviceId, userId);
                if (!deleted)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Service not found or could not be deleted"
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Service deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting service",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<SharableLinkDto>> GenerateSharableLinkAsync(Guid serviceId, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<SharableLinkDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var existingLink = await _sharableLinkRepository.GetByServiceIdAsync(serviceId);
                if (existingLink != null)
                {
                    var existingDto = new SharableLinkDto
                    {
                        Token = existingLink.Token,
                        PublicUrl = $"{_configuration["Frontend:BaseUrl"]}/service/{serviceId}/{existingLink.Token}",
                        CreatedAt = existingLink.CreatedAt
                    };

                    return new ApiResponse<SharableLinkDto>
                    {
                        Success = true,
                        Message = "Sharable link already exists",
                        Data = existingDto
                    };
                }

                var token = await _sharableLinkRepository.CreateAsync(serviceId);

                var sharableLinkDto = new SharableLinkDto
                {
                    Token = token,
                    PublicUrl = $"{_configuration["Frontend:BaseUrl"]}/service/{serviceId}/{token}",
                    CreatedAt = DateTime.UtcNow
                };

                return new ApiResponse<SharableLinkDto>
                {
                    Success = true,
                    Message = "Sharable link generated successfully",
                    Data = sharableLinkDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SharableLinkDto>
                {
                    Success = false,
                    Message = "An error occurred while generating sharable link",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<SharableLinkDto>> RegenerateSharableLinkAsync(Guid serviceId, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<SharableLinkDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var token = await _sharableLinkRepository.RegenerateAsync(serviceId);

                var sharableLinkDto = new SharableLinkDto
                {
                    Token = token,
                    PublicUrl = $"{_configuration["Frontend:BaseUrl"]}/service/{serviceId}/{token}",
                    CreatedAt = DateTime.UtcNow
                };

                return new ApiResponse<SharableLinkDto>
                {
                    Success = true,
                    Message = "Sharable link regenerated successfully",
                    Data = sharableLinkDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SharableLinkDto>
                {
                    Success = false,
                    Message = "An error occurred while regenerating sharable link",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task<ServiceDto> MapToServiceDtoAsync(Service service, SharableLink? sharableLink = null)
        {
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

            return new ServiceDto
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                Duration = service.Duration,
                DurationUnit = service.DurationUnit,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt,
                SharableToken = sharableLink?.Token,
                Images = imageDtos
            };
        }
    }
}