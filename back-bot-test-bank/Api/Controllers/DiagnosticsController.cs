using Api.DTO;
using Api.Filters;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>Diagnostic endpoints for checking each connector from Swagger. Development only.</summary>
    [ApiController]
    [Route("[controller]")]
    [DevelopmentOnly]
    public class DiagnosticsController(ITalkToModelUseCase talkToModel, ILogger<DiagnosticsController> logger) : ControllerBase
    {
        // Errors are not caught here: the global handler turns them into responses.
        [HttpPost("Chat")]
        public async Task<ActionResult<TalkResponse>> Chat([FromBody] TalkRequest request, CancellationToken ct)
        {
            // The message is not logged: it may hold something the customer should not see in logs.
            logger.LogInformation("Diagnostic chat request");
            var reply = await talkToModel.ExecuteAsync(request.Message ?? string.Empty, ct);
            return Ok(new TalkResponse(reply));
        }
    }
}
