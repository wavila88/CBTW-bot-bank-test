using Infra.AI.SemanticKernel;

namespace Tests.Infra
{
    public class OpenAIChatOptionsTests
    {
        [Fact]
        public void Validate_NamesEverySettingThatIsMissing()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new OpenAIChatOptions().Validate());

            Assert.Contains("OpenAI__ApiKey", ex.Message);
            Assert.Contains("Chat__Model", ex.Message);
        }

        [Fact]
        public void Validate_PassesWhenEverySettingIsSet()
        {
            var options = new OpenAIChatOptions { ApiKey = "k", Model = "gpt-4o-mini" };

            options.Validate();
        }
    }
}
