using Application.DTO;
using Application.Prompts;
using Domain.Models;
using Domain.Ports;

namespace Application.UseCases
{
    /// <summary>Answers a customer question inside a conversation: searches the guides, builds the prompt and asks the chat model. It uses only the ports.</summary>
    public sealed class AskQuestionUseCase(IVector vector, IChatModel model, IConversationStore store, TimeProvider time, TimeSpan idle, string rules, int topK) : IAskQuestionUseCase
    {
        public async Task<AnswerDTO> ExecuteAsync(string question, string? conversationId = null, CancellationToken ct = default)
        {
            var id = string.IsNullOrWhiteSpace(conversationId) ? Guid.NewGuid().ToString("N") : conversationId;
            var trimmed = question.Trim();

            // The rule is in Domain; when it says to forget, the conversation is removed and this question starts a new one.
            if (Conversation.ShouldForget(await store.FindAsync(id, ct), time.GetUtcNow(), idle))
                await store.RemoveAsync(id, ct);

            var history = await store.GetTurnsAsync(id, ct);

            // Only a follow-up needs rewriting; the first question is searched as written.
            var searchQuery = history.Count == 0
                ? trimmed
                : QueryRewriter.Clean(await model.CompleteAsync(QueryRewriter.BuildPrompt(history, trimmed), ct), trimmed);

            var chunks = await vector.SearchAsync(searchQuery, topK, ct);
            var reply = (await model.CompleteAsync(PromptBuilder.Build(rules, trimmed, chunks, history, searchQuery), ct)).Trim();

            await store.AddTurnAsync(id, new ChatTurn(trimmed, reply), ct);
            return new AnswerDTO(reply, chunks.Select(c => new SourceDTO(c.Id, c.Score)).ToList(), id);
        }
    }
}
