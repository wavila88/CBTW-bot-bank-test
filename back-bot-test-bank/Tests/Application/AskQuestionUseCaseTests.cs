using Application.Prompts;
using Application.UseCases;
using Domain.Models;
using Domain.Ports;
using Moq;

namespace Tests.Application
{
    public class AskQuestionUseCaseTests
    {
        private const string Rules = "You are the Bank Test assistant. Answer only from the context.";
        private const string Question = "How do I block my card?";
        private const int ConfiguredTopK = 4;

        private static readonly VectorChunk[] Chunks =
        {
            new("KB-01-s2", 0.81, "KB-01", "Checklist", "guide", "Card > Limits: check your daily limit."),
            new("KB-03-q1", 0.52, "KB-03", "FAQ", "faq", "Q: How do I block my card? A: Card > Block.")
        };

        private readonly Mock<IVector> vector = new();
        private readonly Mock<IChatModel> model = new();

        // Every call to the model, in order.
        private readonly List<IReadOnlyList<ChatMessage>> calls = new();

        public AskQuestionUseCaseTests()
        {
            vector.Setup(v => v.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chunks);
            model.Setup(m => m.CompleteAsync(It.IsAny<IReadOnlyList<ChatMessage>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyList<ChatMessage>, CancellationToken>((messages, _) => calls.Add(messages))
                .ReturnsAsync("  Open Card > Block. [KB-03]  ");
        }

        private AskQuestionUseCase UseCase() => new(vector.Object, model.Object, Rules, ConfiguredTopK);

        [Fact]
        public async Task ExecuteAsync_SearchesTheTrimmedQuestionWithTheConfiguredTopK()
        {
            await UseCase().ExecuteAsync("  " + Question + "  ");

            vector.Verify(v => v.SearchAsync(Question, ConfiguredTopK, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ExecuteAsync_SendsTheRulesAsTheSystemMessage()
        {
            await UseCase().ExecuteAsync(Question);

            var sent = Assert.Single(calls);
            Assert.Equal(ChatRole.System, sent[0].Role);
            Assert.Equal(Rules, sent[0].Content);
        }

        [Fact]
        public async Task ExecuteAsync_PutsEveryFragmentWithItsGuideAndIdAndTheQuestionInThePrompt()
        {
            await UseCase().ExecuteAsync(Question);

            var sent = Assert.Single(calls);
            var prompt = sent[sent.Count - 1];
            Assert.Equal(ChatRole.User, prompt.Role);
            foreach (var chunk in Chunks)
            {
                Assert.Contains(chunk.Text, prompt.Content);
                Assert.Contains(chunk.DocId, prompt.Content);
                Assert.Contains(chunk.Id, prompt.Content);
            }
            Assert.Contains(Question, prompt.Content);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsTheTrimmedModelTextAndTheSourcesInSearchOrder()
        {
            var answer = await UseCase().ExecuteAsync(Question);

            Assert.Equal("Open Card > Block. [KB-03]", answer.Text);
            Assert.Equal(new[] { "KB-01-s2", "KB-03-q1" }, answer.Sources.Select(s => s.Id));
            Assert.Equal(new[] { 0.81, 0.52 }, answer.Sources.Select(s => s.Score));
        }

        // The language is asked for inside the same prompt, so a question costs one model call, not two.
        [Fact]
        public async Task ExecuteAsync_CallsTheModelOnceAndEndsThePromptWithTheLanguageReminder()
        {
            await UseCase().ExecuteAsync(Question);

            var sent = Assert.Single(calls);
            Assert.EndsWith(PromptBuilder.LanguageReminder, sent[sent.Count - 1].Content);
        }
    }
}
