using Infra.Vector.Pinne;

namespace Tests.Infra
{
    public class PineconeOptionsTests
    {
        [Fact]
        public void Validate_NamesEverySettingThatIsMissing()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new PineconeOptions().Validate());

            Assert.Contains("Pinecone__ApiKey", ex.Message);
            Assert.Contains("Pinecone__IndexName", ex.Message);
            Assert.Contains("Pinecone__Dimension", ex.Message);
        }

        [Fact]
        public void Validate_PassesWhenEverySettingIsSet()
        {
            var options = new PineconeOptions
            {
                ApiKey = "k",
                IndexName = "customer-service-kb",
                Namespace = "kb",
                EmbeddingModel = "llama-text-embed-v2",
                Dimension = 1024
            };

            options.Validate();
        }
    }
}
