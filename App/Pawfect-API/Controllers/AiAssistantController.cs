using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_API.Attributes;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Models.AiAssistant;
using Pawfect_API.Services.AiAssistantServices;

namespace Pawfect_API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [RateLimit(RateLimitLevel.Restrictive)]
    public class AiAssistantController : ControllerBase
    {
        private readonly IAiAssistantService _aiAssistantService;
        private readonly ILogger<AiAssistantController> _logger;

        public AiAssistantController
        (
            IAiAssistantService animalTypeService,
            ILogger<AiAssistantController> logger
        )
        {
            _aiAssistantService = animalTypeService;
            _logger = logger;
        }

        [HttpPost("completions")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Completions([FromBody] CompletionsRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            CompletionsResponse response = await _aiAssistantService.CompleteionAsync(request);

            return Ok(response);
        }
    }
}
