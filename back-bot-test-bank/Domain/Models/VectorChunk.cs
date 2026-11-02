namespace Domain.Models
{
    /// <summary>A guide fragment returned by the search, with its similarity score.</summary>
    public sealed record VectorChunk(
        string Id,
        double Score,
        string DocId,
        string Section,
        string Type,
        string Text);
}
