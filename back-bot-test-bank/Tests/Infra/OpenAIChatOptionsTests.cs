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
    }
}
