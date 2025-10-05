using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Services;
using AvailabilityApp.Api.Repositories;
using AvailabilityApp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AvailabilityApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceManagementService _serviceManagementService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ServiceImageRepository _serviceImageRepository;
        private readonly IServiceRepository _serviceRepository;

        public ServicesController(
            IServiceManagementService serviceManagementService,
            IBlobStorageService blobStorageService,
            ServiceImageRepository serviceImageRepository,
            IServiceRepository serviceRepository)
        {
            _serviceManagementService = serviceManagementService;
            _blobStorageService = blobStorageService;
            _serviceImageRepository = serviceImageRepository;
            _serviceRepository = serviceRepository;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!.Value);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ServiceDto>>>> GetServices()
        {
            var userId = GetUserId();
            var result = await _serviceManagementService.GetUserServicesAsync(userId);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> GetService(Guid id)
        {
            var userId = GetUserId();
            var result = await _serviceManagementService.GetServiceAsync(id, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> CreateService(CreateServiceDto createServiceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ServiceDto>
                {
                    Success = false,
                    Message = "Invalid input",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserId();
            var result = await _serviceManagementService.CreateServiceAsync(createServiceDto, userId);
            
            if (result.Success)
                return CreatedAtAction(nameof(GetService), new { id = result.Data!.Id }, result);
            
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceDto>>> UpdateService(Guid id, UpdateServiceDto updateServiceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ServiceDto>
                {
                    Success = false,
                    Message = "Invalid input",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserId();
            var result = await _serviceManagementService.UpdateServiceAsync(id, updateServiceDto, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteService(Guid id)
        {
            var userId = GetUserId();
            var result = await _serviceManagementService.DeleteServiceAsync(id, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost("{id}/sharable-link")]
        public async Task<ActionResult<ApiResponse<SharableLinkDto>>> GenerateSharableLink(Guid id)
        {
            var userId = GetUserId();
            var result = await _serviceManagementService.GenerateSharableLinkAsync(id, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost("{id}/regenerate-link")]
        public async Task<ActionResult<ApiResponse<SharableLinkDto>>> RegenerateSharableLink(Guid id)
        {
            var userId = GetUserId();
            var result = await _serviceManagementService.RegenerateSharableLinkAsync(id, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost("{id}/images")]
        public async Task<ActionResult<ApiResponse<ServiceImageDto>>> UploadImage(Guid id, IFormFile file)
        {
            try
            {
                // Validate input
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<ServiceImageDto>
                    {
                        Success = false,
                        Message = "No file provided"
                    });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return BadRequest(new ApiResponse<ServiceImageDto>
                    {
                        Success = false,
                        Message = "Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed."
                    });
                }

                // Validate file size (5MB max)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new ApiResponse<ServiceImageDto>
                    {
                        Success = false,
                        Message = "File size must be less than 5MB"
                    });
                }

                var userId = GetUserId();

                // Verify service ownership
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null || service.UserId != userId)
                {
                    return NotFound(new ApiResponse<ServiceImageDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    });
                }

                // Upload to blob storage
                using var stream = file.OpenReadStream();
                var blobName = await _blobStorageService.UploadAsync(stream, file.FileName, file.ContentType);

                // Save to database
                var serviceImage = new ServiceImage
                {
                    ServiceId = id,
                    BlobName = blobName,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    DisplayOrder = await _serviceImageRepository.GetNextDisplayOrderAsync(id)
                };

                var createdImage = await _serviceImageRepository.CreateAsync(serviceImage);

                var dto = new ServiceImageDto
                {
                    Id = createdImage.Id,
                    ServiceId = createdImage.ServiceId,
                    Url = _blobStorageService.GetBlobUrl(createdImage.BlobName),
                    OriginalFileName = createdImage.OriginalFileName,
                    ContentType = createdImage.ContentType,
                    FileSize = createdImage.FileSize,
                    DisplayOrder = createdImage.DisplayOrder,
                    CreatedAt = createdImage.CreatedAt
                };

                return Ok(new ApiResponse<ServiceImageDto>
                {
                    Success = true,
                    Message = "Image uploaded successfully",
                    Data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ServiceImageDto>
                {
                    Success = false,
                    Message = "An error occurred while uploading the image",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}/images")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ServiceImageDto>>>> GetImages(Guid id)
        {
            var userId = GetUserId();

            // Verify service ownership
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null || service.UserId != userId)
            {
                return NotFound(new ApiResponse<IEnumerable<ServiceImageDto>>
                {
                    Success = false,
                    Message = "Service not found"
                });
            }

            var images = await _serviceImageRepository.GetByServiceIdAsync(id);
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
            });

            return Ok(new ApiResponse<IEnumerable<ServiceImageDto>>
            {
                Success = true,
                Data = imageDtos
            });
        }

        [HttpDelete("{serviceId}/images/{imageId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteImage(Guid serviceId, Guid imageId)
        {
            try
            {
                var userId = GetUserId();

                // Verify service ownership
                var service = await _serviceRepository.GetByIdAsync(serviceId);
                if (service == null || service.UserId != userId)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Service not found"
                    });
                }

                // Get the image
                var image = await _serviceImageRepository.GetByIdAsync(imageId);
                if (image == null || image.ServiceId != serviceId)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Image not found"
                    });
                }

                // Delete from blob storage
                await _blobStorageService.DeleteAsync(image.BlobName);

                // Delete from database
                var deleted = await _serviceImageRepository.DeleteAsync(imageId);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Image deleted successfully",
                    Data = deleted
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the image",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}