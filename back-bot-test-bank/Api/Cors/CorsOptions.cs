using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Api.Cors
{
    /// <summary>The one origin (the front) allowed to call the API from a browser. Set through configuration, never in code.</summary>
    public sealed class CorsOptions
    {
        public string AllowedOrigin { get; set; } = "";

        /// <summary>Fails at startup and names the setting when the origin is missing or is not just scheme, host and port.</summary>
        public void Validate()
        {
            if (!TryGetOrigin(out _))
                throw new InvalidOperationException("Missing or invalid setting: Cors__AllowedOrigin (for example http://localhost:5173).");
        }

        public CorsPolicy ToPolicy()
        {
            Validate();
            TryGetOrigin(out var origin);
            return new CorsPolicyBuilder(origin).WithMethods("POST").AllowAnyHeader().Build();
        }

        private bool TryGetOrigin(out string origin)
        {
            origin = "";
            if (!Uri.TryCreate(AllowedOrigin?.Trim(), UriKind.Absolute, out var uri)) return false;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
            if (uri.AbsolutePath != "/" || uri.Query != "" || uri.Fragment != "") return false;
            origin = uri.GetLeftPart(UriPartial.Authority);
            return true;
        }
    }
}
