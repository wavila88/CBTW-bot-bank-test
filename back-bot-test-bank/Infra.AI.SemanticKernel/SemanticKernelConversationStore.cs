using System.Collections.Concurrent;
using Domain.Models;
using Domain.Ports;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Infra.AI.SemanticKernel
{
    /// <summary>Keeps each conversation in memory as a Semantic Kernel ChatHistory. Conversations survive requests, not a restart.</summary>
    public sealed class SemanticKernelConversationStore(TimeProvider time) : IConversationStore
    {
        private readonly ConcurrentDictionary<string, ChatHistory> conversations = new();
        private readonly ConcurrentDictionary<string, DateTimeOffset> lastActivity = new();

        public Task<IReadOnlyList<ChatTurn>> GetTurnsAsync(string conversationId, CancellationToken ct = default)
        {
            if (!conversations.TryGetValue(conversationId, out var history))
                return Task.FromResult<IReadOnlyList<ChatTurn>>(Array.Empty<ChatTurn>());

            lock (history)
            {
                // Each turn is saved as a user message followed by an assistant message.
                var turns = new List<ChatTurn>();
                for (var i = 0; i + 1 < history.Count; i += 2)
                    turns.Add(new ChatTurn(history[i].Content ?? "", history[i + 1].Content ?? ""));
                return Task.FromResult<IReadOnlyList<ChatTurn>>(turns);
            }
        }

        public Task AddTurnAsync(string conversationId, ChatTurn turn, CancellationToken ct = default)
        {
            var history = conversations.GetOrAdd(conversationId, _ => new ChatHistory());
            lock (history)
            {
                history.AddUserMessage(turn.User);
                history.AddAssistantMessage(turn.Assistant);
            }
            lastActivity[conversationId] = time.GetUtcNow();
            return Task.CompletedTask;
        }

        public Task<Conversation?> FindAsync(string conversationId, CancellationToken ct = default) =>
            Task.FromResult(lastActivity.TryGetValue(conversationId, out var last) ? new Conversation(conversationId, last) : null);

        public Task RemoveAsync(string conversationId, CancellationToken ct = default)
        {
            conversations.TryRemove(conversationId, out _);
            lastActivity.TryRemove(conversationId, out _);
            return Task.CompletedTask;
        }
    }
}
