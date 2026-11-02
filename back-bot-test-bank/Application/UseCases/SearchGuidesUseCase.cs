using Application.DTO;
using Domain.Ports;

namespace Application.UseCases
{
    /// <summary>Searches the guides for the fragments most similar to a question.</summary>
    public sealed class SearchGuidesUseCase(IVector vector, int configuredTopK) : ISearchGuidesUseCase
    {
        public async Task<IReadOnlyList<GuideFragmentDTO>> ExecuteAsync(string query, int? topK = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("The question is empty.", nameof(query));

            var chunks = await vector.SearchAsync(query.Trim(), topK ?? configuredTopK, ct);
            return chunks
                .Select(c => new GuideFragmentDTO(c.Id, c.Score, c.DocId, c.Section, c.Type, c.Text))
                .ToList();
        }
    }
}
