using Domain.Models;

namespace Domain.Ports
{
    /// <summary>Output port: keeps the turns of each conversation, by id. Implemented in Infra.</summary>
    public interface IConversationStore
    {
        /// <summary>Returns the turns of the conversation, oldest first; an unknown id has none.</summary>
        Task<IReadOnlyList<ChatTurn>> GetTurnsAsync(string conversationId, CancellationToken ct = default);

        Task AddTurnAsync(string conversationId, ChatTurn turn, CancellationToken ct = default);

        /// <summary>Returns the conversation with its last activity, or null when the id is unknown.</summary>
        Task<Conversation?> FindAsync(string conversationId, CancellationToken ct = default);

        /// <summary>Forgets the conversation and its turns.</summary>
        Task RemoveAsync(string conversationId, CancellationToken ct = default);
    }
}
