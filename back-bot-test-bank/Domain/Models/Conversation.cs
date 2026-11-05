namespace Domain.Models
{
    /// <summary>A conversation and when it was last used. It owns the idle-expiry rule; the caller gives it the time and the idle time.</summary>
    public sealed record Conversation(string Id, DateTimeOffset LastActivity)
    {
        /// <summary>Business rule: a known conversation idle for longer than the idle time must be forgotten.</summary>
        public static bool ShouldForget(Conversation? found, DateTimeOffset now, TimeSpan idle) =>
            found is not null && now - found.LastActivity > idle;
    }
}
