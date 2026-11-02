using Domain.Models;

namespace Domain.Ports
{
    /// <summary>Output port: finds the guide fragments most similar to a question. Implemented in Infra.</summary>
    public interface IVector
    {
        /// <summary>Returns up to <paramref name="topK"/> fragments, most similar first.</summary>
        /// <exception cref="Exceptions.VectorUnavailableException">The search service could not be reached or rejected the call.</exception>
        Task<IReadOnlyList<VectorChunk>> SearchAsync(string query, int topK, CancellationToken ct = default);
    }
}
