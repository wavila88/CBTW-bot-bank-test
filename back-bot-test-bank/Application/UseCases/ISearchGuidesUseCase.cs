using Application.DTO;

namespace Application.UseCases
{
    public interface ISearchGuidesUseCase
    {
        /// <exception cref="ArgumentException">The question is empty.</exception>
        Task<IReadOnlyList<GuideFragmentDTO>> ExecuteAsync(string query, int? topK = null, CancellationToken ct = default);
    }
}
