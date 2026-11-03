using Api.DTO;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController(IAskQuestionUseCase askQuestion, ILogger<ChatController> logger) : ControllerBase
    {
        // Errors are not caught here: the global handler turns them into responses.
        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request, CancellationToken ct)
        {
            // The message is not logged: it may hold something the customer should not see in logs.
            logger.LogInformation("Chat request received");
            var answer = await askQuestion.ExecuteAsync(request.Message ?? string.Empty, ct);
            return Ok(new ChatResponse(
                answer.Text,
                answer.Sources.Select(s => new SourceResponse(s.Id, s.Score)).ToList()));
        }
    }
}
