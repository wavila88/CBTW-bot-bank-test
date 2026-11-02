using Api.RateLimiting;

namespace Tests.Api
{
    public class RateLimitOptionsTests
    {
        [Fact]
        public void Validate_NamesEverySettingThatIsMissing()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new RateLimitOptions().Validate());

            Assert.Contains("RateLimit__PermitLimit", ex.Message);
            Assert.Contains("RateLimit__WindowSeconds", ex.Message);
        }

        [Fact]
        public void Validate_RejectsZeroOrNegativeValues()
        {
            var options = new RateLimitOptions { PermitLimit = 0, WindowSeconds = -1 };

            var ex = Assert.Throws<InvalidOperationException>(() => options.Validate());

            Assert.Contains("RateLimit__PermitLimit", ex.Message);
            Assert.Contains("RateLimit__WindowSeconds", ex.Message);
        }

        [Fact]
        public void Validate_PassesWhenBothSettingsArePositive()
        {
            var options = new RateLimitOptions { PermitLimit = 5, WindowSeconds = 60 };

            options.Validate();
        }
    }
}
