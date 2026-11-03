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
    }
}
