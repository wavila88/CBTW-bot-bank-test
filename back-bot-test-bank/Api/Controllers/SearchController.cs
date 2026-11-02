using Api.DTO;
using Api.Filters;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>Diagnostic endpoint: shows what the guide search returns for a question.</summary>
    [ApiController]
    [Route("[controller]")]
    [DevelopmentOnly]
    public class SearchController(ISearchGuidesUseCase searchGuides, ILogger<SearchController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SearchResultResponse>>> Get(
            [FromQuery] string query, [FromQuery] int? topK = null, CancellationToken ct = default)
        {
            try
            {
                logger.LogInformation("Diagnostic search with topK {TopK}", topK);
                var fragments = await searchGuides.ExecuteAsync(query, topK, ct);
                return Ok(fragments.Select(f => new SearchResultResponse(f.Id, f.Score, f.DocId, f.Section, f.Type, f.Text)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
