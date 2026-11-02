namespace Domain.Exceptions
{
    /// <summary>The guide search could not be done (service down, timeout, rejected key). Adapters translate their own errors into this one.</summary>
    public sealed class VectorUnavailableException(string message, Exception? inner = null) : Exception(message, inner);
}
