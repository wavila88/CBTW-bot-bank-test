using System.Text;
using Domain.Models;

namespace Application.Prompts
{
    /// <summary>Builds the messages sent to the chat model: the rules first, then the guide fragments and the customer question.</summary>
    public static class PromptBuilder
    {
        // Repeated at the very end of the user message, next to the question: the rules alone do not hold against a prompt
        // in English around guides in Spanish. It also gives the fallback when the language cannot be told.
        public const string LanguageReminder =
            "REMINDER: first identify the language the customer's question is written in, and write your ENTIRE reply in that same " +
            "language (an English question gets an English reply, a Spanish question a Spanish reply, a Portuguese question a " +
            "Portuguese reply), even though the context above is in Spanish. If you cannot tell the language, reply in English.";

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
            user.AppendLine(question.Trim());
            user.AppendLine();
            user.Append(LanguageReminder);

            return new[]
            {
                new ChatMessage(ChatRole.System, rules),
                new ChatMessage(ChatRole.User, user.ToString())
            };
        }
    }
}
