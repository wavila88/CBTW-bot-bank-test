using Domain.Models;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Infra.AI.SemanticKernel
{
    public static class ChatHistoryMapper
    {
        // Turns our messages into Semantic Kernel's history, keeping roles and order.
        public static ChatHistory ToChatHistory(IReadOnlyList<ChatMessage> messages)
        {
            var history = new ChatHistory();
            foreach (var message in messages)
                history.AddMessage(ToAuthorRole(message.Role), message.Content);
            return history;
        }

        private static AuthorRole ToAuthorRole(ChatRole role) => role switch
        {
            ChatRole.System => AuthorRole.System,
            ChatRole.User => AuthorRole.User,
            ChatRole.Assistant => AuthorRole.Assistant,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown chat role.")
        };
    }
}
