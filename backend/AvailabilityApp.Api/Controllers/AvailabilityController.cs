using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AvailabilityApp.Api.Controllers
{
    [Route("api/services/{serviceId}/[controller]")]
    [ApiController]
    [Authorize]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!.Value);
        }

        [HttpGet("patterns")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AvailabilityPatternDto>>>> GetPatterns(Guid serviceId)
        {
            var userId = GetUserId();
            var result = await _availabilityService.GetPatternsAsync(serviceId, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost("patterns")]
        public async Task<ActionResult<ApiResponse<AvailabilityPatternDto>>> CreatePattern(Guid serviceId, CreateAvailabilityPatternDto createPatternDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<AvailabilityPatternDto>
                {
                    Success = false,
                    Message = "Invalid input",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserId();
            var result = await _availabilityService.CreatePatternAsync(serviceId, createPatternDto, userId);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpPut("patterns/{patternId}")]
        public async Task<ActionResult<ApiResponse<AvailabilityPatternDto>>> UpdatePattern(Guid serviceId, Guid patternId, CreateAvailabilityPatternDto updatePatternDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<AvailabilityPatternDto>
                {
                    Success = false,
                    Message = "Invalid input",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserId();
            var result = await _availabilityService.UpdatePatternAsync(patternId, updatePatternDto, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpDelete("patterns/{patternId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePattern(Guid serviceId, Guid patternId)
        {
            var userId = GetUserId();
            var result = await _availabilityService.DeletePatternAsync(patternId, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpGet("slots")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AvailableSlotDto>>>> GetAvailableSlots(
            Guid serviceId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var userId = GetUserId();
            var result = await _availabilityService.GetAvailableSlotsAsync(serviceId, startDate, endDate, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }
    }
}