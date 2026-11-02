namespace Api.RateLimiting
{
    /// <summary>Requests allowed per client IP in each window. Set through configuration, never defaults in code.</summary>
    public sealed class RateLimitOptions
    {
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }

        /// <summary>Fails at startup and names every rate limit setting that is missing or not positive.</summary>
        public void Validate()
        {
            var invalid = new List<string>();
            if (PermitLimit <= 0) invalid.Add("RateLimit__PermitLimit");
            if (WindowSeconds <= 0) invalid.Add("RateLimit__WindowSeconds");

            if (invalid.Count > 0)
                throw new InvalidOperationException("Missing or invalid settings: " + string.Join(", ", invalid));
        }
    }
}
