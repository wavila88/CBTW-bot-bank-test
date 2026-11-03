namespace Domain.Models
{
    /// <summary>One finished exchange of a conversation: what the customer wrote and what the bot answered.</summary>
    public sealed record ChatTurn(string User, string Assistant);
}
