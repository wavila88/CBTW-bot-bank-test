using Domain.Models;
using Infra.AI.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Tests.Infra
{
    public class ChatHistoryMapperTests
    {
        [Fact]
        public void ToChatHistory_KeepsTheRolesAndTheOrder()
        {
            var messages = new[]
            {
                new ChatMessage(ChatRole.System, "rules"),
                new ChatMessage(ChatRole.User, "first question"),
                new ChatMessage(ChatRole.Assistant, "first answer"),
                new ChatMessage(ChatRole.User, "follow-up")
            };

            var history = ChatHistoryMapper.ToChatHistory(messages);

            Assert.Equal(new[] { AuthorRole.System, AuthorRole.User, AuthorRole.Assistant, AuthorRole.User }, history.Select(m => m.Role));
            Assert.Equal(new[] { "rules", "first question", "first answer", "follow-up" }, history.Select(m => m.Content));
        }
    }
}
