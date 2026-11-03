using System.Text;
using Domain.Models;

namespace Application.Prompts
{
    /// <summary>
    /// A follow-up such as "and in dollars?" does not make sense alone, and searching for it as written finds nothing useful.
    /// The model rewrites it so it stands alone, using the earlier conversation.
    /// </summary>
    public static class QueryRewriter
    {
        // Long answers are cut: the rewrite only needs to know what the conversation is about.
        private const int MaxAssistantChars = 600;

        public const string Instructions =
            "You rewrite customer messages into search questions. You will get the previous conversation and the customer's last message. " +
            "Rewrite the LAST message as a complete question that makes sense on its own, without depending on the conversation. " +
            "The rewrite MUST carry the customer's main problem from the WHOLE conversation (for example an unauthorized charge, " +
            "a blocked card, a refund) and every concrete detail (currencies, countries, products, actions already taken), " +
            "not only what was said in the last exchange. Keep the customer's language. " +
            "Reply ONLY with the rewritten question, with no quotes and no explanations. " +
            "If the message already makes sense on its own, return it unchanged.";

        public static IReadOnlyList<ChatMessage> BuildPrompt(IReadOnlyList<ChatTurn> history, string question)
        {
            var user = new StringBuilder();
            user.AppendLine("PREVIOUS CONVERSATION:");
            foreach (var turn in history)
            {
                user.AppendLine($"Customer: {turn.User}");
                user.AppendLine($"Assistant: {Shorten(turn.Assistant)}");
            }
            user.AppendLine();
            user.AppendLine("CUSTOMER'S LAST QUESTION:");
            user.Append(question.Trim());

            return new[]
            {
                new ChatMessage(ChatRole.System, Instructions),
                new ChatMessage(ChatRole.User, user.ToString())
            };
        }

        // Keeps the first line without quotes; if the model returned nothing useful, the original question is used.
        public static string Clean(string? output, string fallback)
        {
            var line = (output ?? "")
                .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            if (line is null) return fallback;

            line = line.Trim().Trim('"', '“', '”', '«', '»').Trim();
            return line.Length == 0 ? fallback : line;
        }

        private static string Shorten(string text) =>
            text.Length <= MaxAssistantChars ? text : text[..MaxAssistantChars].TrimEnd() + "...";
    }
}
