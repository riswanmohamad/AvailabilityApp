using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Models;
using AvailabilityApp.Api.Repositories;
using AvailabilityApp.Api.Utils;

namespace AvailabilityApp.Api.Services
{
    public interface IAvailabilityService
    {
        Task<ApiResponse<IEnumerable<AvailabilityPatternDto>>> GetPatternsAsync(Guid serviceId, Guid userId);
        Task<ApiResponse<AvailabilityPatternDto>> CreatePatternAsync(Guid serviceId, CreateAvailabilityPatternDto createPatternDto, Guid userId);
        Task<ApiResponse<AvailabilityPatternDto>> UpdatePatternAsync(Guid patternId, CreateAvailabilityPatternDto updatePatternDto, Guid userId);
        Task<ApiResponse<bool>> DeletePatternAsync(Guid patternId, Guid userId);
        Task<ApiResponse<IEnumerable<AvailableSlotDto>>> GetAvailableSlotsAsync(Guid serviceId, DateTime startDate, DateTime endDate, Guid userId);
        Task<bool> RegenerateSlots(Guid patternId);
    }

    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IExceptionRepository _exceptionRepository;
        private readonly ISlotGenerator _slotGenerator;

        public AvailabilityService(
            IAvailabilityRepository availabilityRepository,
            IServiceRepository serviceRepository,
            IExceptionRepository exceptionRepository,
            ISlotGenerator slotGenerator)
        {
            _availabilityRepository = availabilityRepository;
            _serviceRepository = serviceRepository;
            _exceptionRepository = exceptionRepository;
            _slotGenerator = slotGenerator;
        }

        public async Task<ApiResponse<IEnumerable<AvailabilityPatternDto>>> GetPatternsAsync(Guid serviceId, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<IEnumerable<AvailabilityPatternDto>>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var patterns = await _availabilityRepository.GetPatternsByServiceIdAsync(serviceId);
                var patternDtos = patterns.Select(p => new AvailabilityPatternDto
                {
                    Id = p.Id,
                    SlotType = p.SlotType,
                    SlotDuration = p.SlotDuration,
                    StartTime = p.StartTime,
                    EndTime = p.EndTime,
                    DaysOfWeek = p.DaysOfWeek,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                });

                return new ApiResponse<IEnumerable<AvailabilityPatternDto>>
                {
                    Success = true,
                    Data = patternDtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<AvailabilityPatternDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching availability patterns",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<AvailabilityPatternDto>> CreatePatternAsync(Guid serviceId, CreateAvailabilityPatternDto createPatternDto, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<AvailabilityPatternDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var pattern = new AvailabilityPattern
                {
                    ServiceId = serviceId,
                    SlotType = createPatternDto.SlotType,
                    SlotDuration = createPatternDto.SlotDuration,
                    StartTime = createPatternDto.StartTime,
                    EndTime = createPatternDto.EndTime,
                    DaysOfWeek = createPatternDto.DaysOfWeek,
                    StartDate = createPatternDto.StartDate,
                    EndDate = createPatternDto.EndDate
                };

                var patternId = await _availabilityRepository.CreatePatternAsync(pattern);
                pattern.Id = patternId;

                // Generate slots for the next 3 months
                var endDate = createPatternDto.EndDate ?? DateTime.UtcNow.AddMonths(3);
                var slots = _slotGenerator.GenerateSlots(pattern, createPatternDto.StartDate, endDate);
                await _availabilityRepository.CreateSlotsAsync(slots);

                var patternDto = new AvailabilityPatternDto
                {
                    Id = pattern.Id,
                    SlotType = pattern.SlotType,
                    SlotDuration = pattern.SlotDuration,
                    StartTime = pattern.StartTime,
                    EndTime = pattern.EndTime,
                    DaysOfWeek = pattern.DaysOfWeek,
                    StartDate = pattern.StartDate,
                    EndDate = pattern.EndDate
                };

                return new ApiResponse<AvailabilityPatternDto>
                {
                    Success = true,
                    Message = "Availability pattern created successfully",
                    Data = patternDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AvailabilityPatternDto>
                {
                    Success = false,
                    Message = "An error occurred while creating availability pattern",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<AvailabilityPatternDto>> UpdatePatternAsync(Guid patternId, CreateAvailabilityPatternDto updatePatternDto, Guid userId)
        {
            try
            {
                var existingPattern = await _availabilityRepository.GetPatternByIdAsync(patternId);
                if (existingPattern == null)
                {
                    return new ApiResponse<AvailabilityPatternDto>
                    {
                        Success = false,
                        Message = "Availability pattern not found"
                    };
                }

                var service = await _serviceRepository.GetByIdAndUserIdAsync(existingPattern.ServiceId, userId);
                if (service == null)
                {
                    return new ApiResponse<AvailabilityPatternDto>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                existingPattern.SlotType = updatePatternDto.SlotType;
                existingPattern.SlotDuration = updatePatternDto.SlotDuration;
                existingPattern.StartTime = updatePatternDto.StartTime;
                existingPattern.EndTime = updatePatternDto.EndTime;
                existingPattern.DaysOfWeek = updatePatternDto.DaysOfWeek;
                existingPattern.StartDate = updatePatternDto.StartDate;
                existingPattern.EndDate = updatePatternDto.EndDate;

                var updated = await _availabilityRepository.UpdatePatternAsync(existingPattern);
                if (!updated)
                {
                    return new ApiResponse<AvailabilityPatternDto>
                    {
                        Success = false,
                        Message = "Failed to update availability pattern"
                    };
                }

                // Regenerate slots
                await RegenerateSlots(patternId);

                var patternDto = new AvailabilityPatternDto
                {
                    Id = existingPattern.Id,
                    SlotType = existingPattern.SlotType,
                    SlotDuration = existingPattern.SlotDuration,
                    StartTime = existingPattern.StartTime,
                    EndTime = existingPattern.EndTime,
                    DaysOfWeek = existingPattern.DaysOfWeek,
                    StartDate = existingPattern.StartDate,
                    EndDate = existingPattern.EndDate
                };

                return new ApiResponse<AvailabilityPatternDto>
                {
                    Success = true,
                    Message = "Availability pattern updated successfully",
                    Data = patternDto
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AvailabilityPatternDto>
                {
                    Success = false,
                    Message = "An error occurred while updating availability pattern",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeletePatternAsync(Guid patternId, Guid userId)
        {
            try
            {
                var pattern = await _availabilityRepository.GetPatternByIdAsync(patternId);
                if (pattern == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Availability pattern not found"
                    };
                }

                var service = await _serviceRepository.GetByIdAndUserIdAsync(pattern.ServiceId, userId);
                if (service == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                // Delete associated slots first
                await _availabilityRepository.DeleteSlotsByPatternIdAsync(patternId);

                // Delete pattern
                var deleted = await _availabilityRepository.DeletePatternAsync(patternId, pattern.ServiceId);
                if (!deleted)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to delete availability pattern"
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Availability pattern deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting availability pattern",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<AvailableSlotDto>>> GetAvailableSlotsAsync(Guid serviceId, DateTime startDate, DateTime endDate, Guid userId)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAndUserIdAsync(serviceId, userId);
                if (service == null)
                {
                    return new ApiResponse<IEnumerable<AvailableSlotDto>>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                var slots = await _availabilityRepository.GetSlotsByServiceIdAsync(serviceId, startDate, endDate);
                var exceptions = await _exceptionRepository.GetActiveExceptionsForPeriodAsync(serviceId, startDate, endDate);

                var availableSlots = FilterSlotsByExceptions(slots, exceptions);

                var slotDtos = availableSlots.Select(s => new AvailableSlotDto
                {
                    Id = s.Id,
                    StartDateTime = s.StartDateTime,
                    EndDateTime = s.EndDateTime,
                    SlotType = s.SlotType,
                    IsAvailable = s.IsAvailable
                });

                return new ApiResponse<IEnumerable<AvailableSlotDto>>
                {
                    Success = true,
                    Data = slotDtos
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<AvailableSlotDto>>
                {
                    Success = false,
                    Message = "An error occurred while fetching available slots",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<bool> RegenerateSlots(Guid patternId)
        {
            try
            {
                var pattern = await _availabilityRepository.GetPatternByIdAsync(patternId);
                if (pattern == null) return false;

                // Delete existing slots
                await _availabilityRepository.DeleteSlotsByPatternIdAsync(patternId);

                // Generate new slots for the next 3 months
                var endDate = pattern.EndDate ?? DateTime.UtcNow.AddMonths(3);
                var slots = _slotGenerator.GenerateSlots(pattern, pattern.StartDate, endDate);
                await _availabilityRepository.CreateSlotsAsync(slots);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private IEnumerable<AvailableSlot> FilterSlotsByExceptions(IEnumerable<AvailableSlot> slots, IEnumerable<ServiceException> exceptions)
        {
            var filteredSlots = new List<AvailableSlot>();

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