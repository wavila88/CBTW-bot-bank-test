using Domain.Models;
using Domain.Ports;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Infra.AI.SemanticKernel
{
    /// <summary>Chat model through Semantic Kernel's chat completion service. No Semantic Kernel type leaves this project.</summary>
    public sealed class SemanticKernelChatModel(IChatCompletionService service) : IChatModel
    {
        public async Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct = default)
        {
            var reply = await service.GetChatMessageContentAsync(ChatHistoryMapper.ToChatHistory(messages), cancellationToken: ct);
            return reply.Content ?? "";
        }
    }
}
