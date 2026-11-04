using ApiCorsOptions = Api.Cors.CorsOptions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Tests.Api
{
    public class CorsOptionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("localhost:5173")]
        [InlineData("http://localhost:5173/chat")]
        public void Validate_RejectsAMissingOrMalformedOriginAndNamesTheSetting(string? origin)
        {
            var options = new ApiCorsOptions { AllowedOrigin = origin! };

            var ex = Assert.Throws<InvalidOperationException>(() => options.Validate());

            Assert.Contains("Cors__AllowedOrigin", ex.Message);
        }

        [Fact]
        public void Policy_GivesTheCorsHeaderToTheAllowedOriginOnly()
        {
            var policy = new ApiCorsOptions { AllowedOrigin = "http://localhost:5173/" }.ToPolicy();
            var service = new CorsService(Options.Create(new Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions()), NullLoggerFactory.Instance);

            var allowed = service.EvaluatePolicy(RequestFrom("http://localhost:5173"), policy);
            var other = service.EvaluatePolicy(RequestFrom("http://evil.example.com"), policy);

            Assert.True(allowed.IsOriginAllowed);
            Assert.False(other.IsOriginAllowed);
        }

        private static HttpContext RequestFrom(string origin)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Headers.Origin = origin;
            return context;
        }
    }
}
