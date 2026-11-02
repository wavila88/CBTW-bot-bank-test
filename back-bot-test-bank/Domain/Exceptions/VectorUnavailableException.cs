namespace Domain.Exceptions
{
    /// <summary>The vector search failed. For now every Pinecone failure uses the Unavailable code; Phase 3b gives each failure its own code.</summary>
    public sealed class VectorUnavailableException(string message, Exception? inner = null)
        : DomainException(ErrorCode.Unavailable, message, inner);
}
