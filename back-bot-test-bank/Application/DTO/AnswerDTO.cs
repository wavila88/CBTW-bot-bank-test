namespace Application.DTO
{
    public sealed record AnswerDTO(string Text, IReadOnlyList<SourceDTO> Sources);
}
