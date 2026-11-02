namespace Domain.Exceptions
{
    /// <summary>What went wrong, in terms the bot understands. Api decides the HTTP status and the customer message for each code.</summary>
    public enum ErrorCode
    {
        InvalidRequest,
        Unreachable,
        Timeout,
        Rejected,
        RateLimited,
        Unavailable,
        NotFound,
        Malformed
    }
}
