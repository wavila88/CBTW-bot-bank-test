namespace Api.DTO
{
    public sealed record ChatResponse(string Answer, IReadOnlyList<SourceResponse> Sources, string ConversationId);
}
