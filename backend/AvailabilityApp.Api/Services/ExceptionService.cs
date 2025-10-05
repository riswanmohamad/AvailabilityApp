using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Models;
using AvailabilityApp.Api.Repositories;

namespace AvailabilityApp.Api.Services
{
    public interface IExceptionService
    {
        Task<ApiResponse<IEnumerable<ExceptionDto>>> GetExceptionsAsync(Guid serviceId, Guid userId);
        Task<ApiResponse<ExceptionDto>> CreateExceptionAsync(Guid serviceId, CreateExceptionDto createExceptionDto, Guid userId);
        Task<ApiResponse<ExceptionDto>> UpdateExceptionAsync(Guid exceptionId, CreateExceptionDto updateExceptionDto, Guid userId);
        Task<ApiResponse<bool>> DeleteExceptionAsync(Guid exceptionId, Guid userId);
    }

    public class ExceptionService : IExceptionService
    {
        private readonly IExceptionRepository _exceptionRepository;
        private readonly IServiceRepository _serviceRepository;

        public ExceptionService(IExceptionRepository exceptionRepository, IServiceRepository serviceRepository)
        {
            _exceptionRepository = exceptionRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<ApiResponse<IEnumerable<ExceptionDto>>> GetExceptionsAsync(Guid serviceId, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<IEnumerable<ExceptionDto>>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var exceptions = await _exceptionRepository.GetByServiceIdAsync(serviceId);
                var exceptionDtos = exceptions.Select(e => new ExceptionDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartDateTime = e.StartDateTime,
                    EndDateTime = e.EndDateTime,
                    ExceptionType = e.ExceptionType,
                    RecurringYearly = e.RecurringYearly
                });

                return new ApiResponse<IEnumerable<ExceptionDto>>
                {
                    Success = true,
                    Data = exceptionDtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<ExceptionDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching exceptions",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ExceptionDto>> CreateExceptionAsync(Guid serviceId, CreateExceptionDto createExceptionDto, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<ExceptionDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var exception = new ServiceException
                {
                    ServiceId = serviceId,
                    Title = createExceptionDto.Title,
                    Description = createExceptionDto.Description,
                    StartDateTime = createExceptionDto.StartDateTime,
                    EndDateTime = createExceptionDto.EndDateTime,
                    ExceptionType = createExceptionDto.ExceptionType,
                    RecurringYearly = createExceptionDto.RecurringYearly
                };

                var exceptionId = await _exceptionRepository.CreateAsync(exception);
                exception.Id = exceptionId;

                var exceptionDto = new ExceptionDto
                {
                    Id = exception.Id,
                    Title = exception.Title,
                    Description = exception.Description,
                    StartDateTime = exception.StartDateTime,
                    EndDateTime = exception.EndDateTime,
                    ExceptionType = exception.ExceptionType,
                    RecurringYearly = exception.RecurringYearly
                };

                return new ApiResponse<ExceptionDto>
                {
                    Success = true,
                    Message = "Exception created successfully",
                    Data = exceptionDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ExceptionDto>
                {
                    Success = false,
                    Message = "An error occurred while creating exception",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ExceptionDto>> UpdateExceptionAsync(Guid exceptionId, CreateExceptionDto updateExceptionDto, Guid userId)
        {
            try
            {
                var existingException = await _exceptionRepository.GetByIdAsync(exceptionId);
                if (existingException == null)
                {
                    return new ApiResponse<ExceptionDto>
                    {
                        Success = false,
                        Message = "Exception not found"
                    };
                }

                var service = await _serviceRepository.GetByIdAndUserIdAsync(existingException.ServiceId, userId);
                if (service == null)
                {
                    return new ApiResponse<ExceptionDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                existingException.Title = updateExceptionDto.Title;
                existingException.Description = updateExceptionDto.Description;
                existingException.StartDateTime = updateExceptionDto.StartDateTime;
                existingException.EndDateTime = updateExceptionDto.EndDateTime;
                existingException.ExceptionType = updateExceptionDto.ExceptionType;
                existingException.RecurringYearly = updateExceptionDto.RecurringYearly;

                var updated = await _exceptionRepository.UpdateAsync(existingException);
                if (!updated)
                {
                    return new ApiResponse<ExceptionDto>
                    {
                        Success = false,
                        Message = "Failed to update exception"
                    };
                }

                var exceptionDto = new ExceptionDto
                {
                    Id = existingException.Id,
                    Title = existingException.Title,
                    Description = existingException.Description,
                    StartDateTime = existingException.StartDateTime,
                    EndDateTime = existingException.EndDateTime,
                    ExceptionType = existingException.ExceptionType,
                    RecurringYearly = existingException.RecurringYearly
                };

                return new ApiResponse<ExceptionDto>
                {
                    Success = true,
                    Message = "Exception updated successfully",
                    Data = exceptionDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ExceptionDto>
                {
                    Success = false,
                    Message = "An error occurred while updating exception",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteExceptionAsync(Guid exceptionId, Guid userId)
        {
            try
            {
                var exception = await _exceptionRepository.GetByIdAsync(exceptionId);
                if (exception == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Exception not found"
                    };
                }

                var service = await _serviceRepository.GetByIdAndUserIdAsync(exception.ServiceId, userId);
                if (service == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var deleted = await _exceptionRepository.DeleteAsync(exceptionId, exception.ServiceId);
                if (!deleted)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to delete exception"
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Exception deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting exception",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}