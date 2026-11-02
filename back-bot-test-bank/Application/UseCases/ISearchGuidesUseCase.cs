using Application.DTO;

namespace Application.UseCases
{
    public interface ISearchGuidesUseCase
    {
        /// <exception cref="Domain.Exceptions.DomainException">Code InvalidRequest: the question is empty.</exception>
        Task<IReadOnlyList<GuideFragmentDTO>> ExecuteAsync(string query, int? topK = null, CancellationToken ct = default);
    }
}
