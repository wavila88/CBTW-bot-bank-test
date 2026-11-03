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
        private const string FollowUp = "and in dollars?";
        private const int ConfiguredTopK = 4;

        private static readonly VectorChunk[] Chunks =
        {
            new("KB-01-s2", 0.81, "KB-01", "Checklist", "guide", "Card > Limits: check your daily limit."),
            new("KB-03-q1", 0.52, "KB-03", "FAQ", "faq", "Q: How do I block my card? A: Card > Block.")
        };

        private static readonly ChatTurn[] Earlier =
        {
            new("How do I pay with my card abroad?", "Use Card > Payments abroad. [KB-02]")
        };

        private readonly Mock<IVector> vector = new();
        private readonly Mock<IChatModel> model = new();
        private readonly Mock<IConversationStore> store = new();

        // Every call to the model, in order: the rewrite of a follow-up first (if any), then the answer.
        private readonly List<IReadOnlyList<ChatMessage>> calls = new();
        private IReadOnlyList<ChatTurn> previousTurns = Array.Empty<ChatTurn>();
        private readonly string rewritten = "How do I pay with my card in dollars abroad?";

        public AskQuestionUseCaseTests()
        {
            vector.Setup(v => v.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chunks);
            store.Setup(s => s.GetTurnsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => previousTurns);
            model.Setup(m => m.CompleteAsync(It.IsAny<IReadOnlyList<ChatMessage>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ChatMessage> messages, CancellationToken _) =>
                {
                    calls.Add(messages);
                    return messages[0].Content == QueryRewriter.Instructions
                        ? rewritten
                        : "  Open Card > Block. [KB-03]  ";
                });
        }

        private AskQuestionUseCase UseCase() => new(vector.Object, model.Object, store.Object, Rules, ConfiguredTopK);

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

        // The language is asked for inside the same prompt, so a first question costs one model call, not two.
        [Fact]
        public async Task ExecuteAsync_CallsTheModelOnceAndEndsThePromptWithTheLanguageReminder()
        {
            await UseCase().ExecuteAsync(Question);

            var sent = Assert.Single(calls);
            Assert.EndsWith(PromptBuilder.LanguageReminder, sent[sent.Count - 1].Content);
        }

        [Fact]
        public async Task ExecuteAsync_StartsANewConversationWhenNoIdIsGiven()
        {
            var answer = await UseCase().ExecuteAsync(Question);

            Assert.False(string.IsNullOrWhiteSpace(answer.ConversationId));
            store.Verify(s => s.AddTurnAsync(answer.ConversationId, It.IsAny<ChatTurn>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ExecuteAsync_KeepsTheGivenConversationId()
        {
            var answer = await UseCase().ExecuteAsync(Question, "conversation-1");

            Assert.Equal("conversation-1", answer.ConversationId);
            store.Verify(s => s.GetTurnsAsync("conversation-1", It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ExecuteAsync_SavesTheQuestionAndTheTrimmedAnswerAsTheNewTurn()
        {
            await UseCase().ExecuteAsync("  " + Question + "  ", "conversation-1");

            store.Verify(s => s.AddTurnAsync(
                "conversation-1", new ChatTurn(Question, "Open Card > Block. [KB-03]"), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ExecuteAsync_RewritesAFollowUpAndSearchesWithTheRewrittenText()
        {
            previousTurns = Earlier;

            await UseCase().ExecuteAsync(FollowUp, "conversation-1");

            vector.Verify(v => v.SearchAsync(rewritten, ConfiguredTopK, It.IsAny<CancellationToken>()));
            var rewrite = calls[0];
            Assert.Equal(QueryRewriter.Instructions, rewrite[0].Content);
            Assert.Contains(Earlier[0].User, rewrite[1].Content);
            Assert.Contains(FollowUp, rewrite[1].Content);
        }

        [Fact]
        public async Task ExecuteAsync_SendsEarlierTurnsAsUserAndAssistantMessagesBeforeTheQuestion()
        {
            previousTurns = Earlier;

            await UseCase().ExecuteAsync(FollowUp, "conversation-1");

            var sent = calls[^1];
            Assert.Equal(new[] { ChatRole.System, ChatRole.User, ChatRole.Assistant, ChatRole.User }, sent.Select(m => m.Role));
            Assert.Equal(Earlier[0].User, sent[1].Content);
            Assert.Equal(Earlier[0].Assistant, sent[2].Content);
            Assert.Contains(FollowUp, sent[3].Content);
        }

        // The model reads "I need my money back" next to how the search understood it: the same message about an unauthorized charge.
        [Fact]
        public async Task ExecuteAsync_TellsTheModelHowTheFollowUpWasUnderstood()
        {
            previousTurns = Earlier;

            await UseCase().ExecuteAsync(FollowUp, "conversation-1");

            var prompt = calls[^1][^1].Content;
            Assert.Contains($"Interpretation in context: {rewritten}", prompt);
        }

        [Fact]
        public async Task ExecuteAsync_AddsNoInterpretationToAFirstQuestion()
        {
            await UseCase().ExecuteAsync(Question);

            var sent = Assert.Single(calls);
            Assert.DoesNotContain("Interpretation in context", sent[sent.Count - 1].Content);
        }
    }
}
