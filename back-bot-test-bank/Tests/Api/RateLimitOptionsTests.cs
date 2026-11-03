using Api.RateLimiting;

namespace Tests.Api
{
    public class RateLimitOptionsTests
    {
        [Fact]
        public void Validate_RejectsMissingZeroOrNegativeValuesAndNamesThem()
        {
            var options = new RateLimitOptions { PermitLimit = 0, WindowSeconds = -1 };

            var ex = Assert.Throws<InvalidOperationException>(() => options.Validate());

            Assert.Contains("RateLimit__PermitLimit", ex.Message);
            Assert.Contains("RateLimit__WindowSeconds", ex.Message);
        }
    }
}
