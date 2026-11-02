namespace Domain.Exceptions
{
    /// <summary>A failure the bot can name. Carries a code and an internal message for the log, never text for the customer.</summary>
    public class DomainException(ErrorCode code, string message, Exception? inner = null) : Exception(message, inner)
    {
        public ErrorCode Code { get; } = code;
    }
}
