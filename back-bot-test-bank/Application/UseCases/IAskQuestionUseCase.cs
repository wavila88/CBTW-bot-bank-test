using Application.DTO;

namespace Application.UseCases
{
    public interface IAskQuestionUseCase
    {
        Task<AnswerDTO> ExecuteAsync(string question, CancellationToken ct = default);
    }
}
