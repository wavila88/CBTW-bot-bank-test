using System.Text;
using Domain.Models;

namespace Application.Prompts
{
    /// <summary>Builds the messages sent to the chat model: the rules first, then the guide fragments and the customer question.</summary>
    public static class PromptBuilder
    {
        public static IReadOnlyList<ChatMessage> Build(string rules, string question, IReadOnlyList<VectorChunk> context)
        {
            var user = new StringBuilder();
            user.AppendLine("CONTEXT (fragments from the internal guides; use only this to answer):");
            user.AppendLine();
            for (var i = 0; i < context.Count; i++)
            {
                var chunk = context[i];
                user.AppendLine($"### Fragment {i + 1} - guide {chunk.DocId} - id {chunk.Id}");
                user.AppendLine(chunk.Text);
                user.AppendLine();
            }
            user.AppendLine("CUSTOMER QUESTION:");
            user.Append(question.Trim());

            return new[]
            {
                new ChatMessage(ChatRole.System, rules),
                new ChatMessage(ChatRole.User, user.ToString())
            };
        }
    }
}
