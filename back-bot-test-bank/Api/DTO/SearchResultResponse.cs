namespace Api.DTO
{
    public sealed record SearchResultResponse(string Id, double Score, string DocId, string Section, string Type, string Text);
}
