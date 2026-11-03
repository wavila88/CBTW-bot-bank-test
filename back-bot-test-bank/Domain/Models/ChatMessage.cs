namespace Domain.Models
{
    /// <summary>One message of the conversation sent to the chat model.</summary>
    public sealed record ChatMessage(ChatRole Role, string Content);
}
