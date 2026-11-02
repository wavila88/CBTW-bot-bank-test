using Application.DTO;
using Application.UseCases;
using Domain.Exceptions;
using Domain.Models;
using Domain.Ports;
using Moq;

namespace Tests.Application
{
    public class SearchGuidesUseCaseTests
    {
        private const int ConfiguredTopK = 4;

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExecuteAsync_RejectsAnEmptyQuestionWithoutSearching(string query)
        {
            var vector = new Mock<IVector>();

            var ex = await Assert.ThrowsAsync<DomainException>(() => new SearchGuidesUseCase(vector.Object, ConfiguredTopK).ExecuteAsync(query));
            Assert.Equal(ErrorCode.InvalidRequest, ex.Code);

            vector.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_SearchesTheTrimmedQuestionWithTheConfiguredTopK()
        {
            var vector = new Mock<IVector>();
            vector.Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<VectorChunk>());

            await new SearchGuidesUseCase(vector.Object, ConfiguredTopK).ExecuteAsync("  block my card  ");

            vector.Verify(r => r.SearchAsync("block my card", ConfiguredTopK, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ExecuteAsync_UsesTheTopKOfTheCallerWhenGiven()
        {
            var vector = new Mock<IVector>();
            vector.Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<VectorChunk>());

            await new SearchGuidesUseCase(vector.Object, ConfiguredTopK).ExecuteAsync("hello", 2);

            vector.Verify(r => r.SearchAsync("hello", 2, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsTheFragmentsAsApplicationDtos()
        {
            var vector = new Mock<IVector>();
            vector.Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new VectorChunk("KB-01-s2", 0.81, "KB-01", "Checklist", "guide", "Card > Limits") });

            IReadOnlyList<GuideFragmentDTO> fragments = await new SearchGuidesUseCase(vector.Object, ConfiguredTopK).ExecuteAsync("hello", 3);

            var fragment = Assert.Single(fragments);
            Assert.Equal("KB-01-s2", fragment.Id);
            Assert.Equal("KB-01", fragment.DocId);
            Assert.Equal(0.81, fragment.Score);
        }
    }
}
