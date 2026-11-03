using Domain.Models;

namespace Domain.Ports
{
    /// <summary>Output port: sends a conversation to the chat model and returns its reply. Implemented in Infra.</summary>
    public interface IChatModel
    {
        Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct = default);
    }
}
