using AvailabilityApp.Api.DTOs;
using AvailabilityApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly IPublicService _publicService;

        public PublicController(IPublicService publicService)
        {
            _publicService = publicService;
        }

        [HttpGet("service/{token}")]
        public async Task<ActionResult<ApiResponse<PublicServiceDto>>> GetPublicService(string token)
        {
            var result = await _publicService.GetPublicServiceAsync(token);
            
            if (result.Success)
                return Ok(result);
            
            return NotFound(result);
        }
    }
}