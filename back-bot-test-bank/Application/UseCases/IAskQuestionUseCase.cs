using Application.DTO;

namespace Application.UseCases
{
    public interface IAskQuestionUseCase
    {
        /// <param name="conversationId">The conversation to continue; a new one is started when it is null or unknown.</param>
        Task<AnswerDTO> ExecuteAsync(string question, string? conversationId = null, CancellationToken ct = default);
    }
}
