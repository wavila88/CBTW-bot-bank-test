using Application.DTO;
using Application.Prompts;
using Domain.Ports;

namespace Application.UseCases
{
    /// <summary>Answers a customer question: searches the guides, builds the prompt and asks the chat model. It uses only the ports.</summary>
    public sealed class AskQuestionUseCase(IVector vector, IChatModel model, string rules, int topK) : IAskQuestionUseCase
    {
        public async Task<AnswerDTO> ExecuteAsync(string question, CancellationToken ct = default)
        {

            var chunks = await vector.SearchAsync(question.Trim(), topK, ct);
            var reply = await model.CompleteAsync(PromptBuilder.Build(rules, question, chunks), ct);

            return new AnswerDTO(reply.Trim(), chunks.Select(c => new SourceDTO(c.Id, c.Score)).ToList());
        }
    }
}
