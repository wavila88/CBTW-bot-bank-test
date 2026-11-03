using Domain.Exceptions;
using Domain.Models;
using Domain.Ports;

namespace Application.UseCases
{
    /// <summary>Sends one message to the chat model and returns its reply. Used by the Development diagnostic endpoint.</summary>
    public sealed class TalkToModelUseCase(IChatModel model) : ITalkToModelUseCase
    {
        public async Task<string> ExecuteAsync(string message, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new DomainException(ErrorCode.InvalidRequest, "The message is empty.");

            return await model.CompleteAsync(new[] { new ChatMessage(ChatRole.User, message.Trim()) }, ct);
        }
    }
}
