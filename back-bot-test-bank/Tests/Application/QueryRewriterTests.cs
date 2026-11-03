using Application.Prompts;
using Domain.Models;

namespace Tests.Application
{
    public class QueryRewriterTests
    {
        [Fact]
        public void BuildPrompt_ShowsThePreviousTurnsAndEndsWithTheLastQuestion()
        {
            var history = new[] { new ChatTurn("How do I pay abroad?", "Use Card > Payments abroad.") };

            var messages = QueryRewriter.BuildPrompt(history, "  and in dollars?  ");

            Assert.Equal(ChatRole.System, messages[0].Role);
            Assert.Equal(QueryRewriter.Instructions, messages[0].Content);
            Assert.Contains("How do I pay abroad?", messages[1].Content);
            Assert.Contains("Use Card > Payments abroad.", messages[1].Content);
            Assert.EndsWith("and in dollars?", messages[1].Content);
        }

        [Theory]
        [InlineData("How do I pay in dollars abroad?", "How do I pay in dollars abroad?")]
        [InlineData("  \"How do I pay in dollars abroad?\"  ", "How do I pay in dollars abroad?")]
        [InlineData("How do I pay in dollars abroad?\nIt is a follow-up.", "How do I pay in dollars abroad?")]
        [InlineData("", "and in dollars?")]
        [InlineData(null, "and in dollars?")]
        public void Clean_KeepsTheFirstLineWithoutQuotesOrFallsBackToTheOriginalQuestion(string? output, string expected)
        {
            Assert.Equal(expected, QueryRewriter.Clean(output, "and in dollars?"));
        }
    }
}
