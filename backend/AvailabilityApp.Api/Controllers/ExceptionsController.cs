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
    public class ExceptionsController : ControllerBase
    {
        private readonly IExceptionService _exceptionService;

        public ExceptionsController(IExceptionService exceptionService)
        {
            _exceptionService = exceptionService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!.Value);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ExceptionDto>>>> GetExceptions(Guid serviceId)
        {
            var userId = GetUserId();
            var result = await _exceptionService.GetExceptionsAsync(serviceId, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ExceptionDto>>> CreateException(Guid serviceId, CreateExceptionDto createExceptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ExceptionDto>
                {
                    Success = false,
                    Message = "Invalid input",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserId();
            var result = await _exceptionService.CreateExceptionAsync(serviceId, createExceptionDto, userId);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpPut("{exceptionId}")]
        public async Task<ActionResult<ApiResponse<ExceptionDto>>> UpdateException(Guid serviceId, Guid exceptionId, CreateExceptionDto updateExceptionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ExceptionDto>
                {
                    Success = false,
                    Message = "Invalid input",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetUserId();
            var result = await _exceptionService.UpdateExceptionAsync(exceptionId, updateExceptionDto, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpDelete("{exceptionId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteException(Guid serviceId, Guid exceptionId)
        {
            var userId = GetUserId();
            var result = await _exceptionService.DeleteExceptionAsync(exceptionId, userId);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }
    }
}