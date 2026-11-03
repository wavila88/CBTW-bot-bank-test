namespace Infra.AI.SemanticKernel
{
    public sealed class OpenAIChatOptions
    {
        // Set through environment variables (OpenAI__ApiKey, Chat__Model) or appsettings.Local.json. No defaults in code.
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "";

        /// <summary>Fails at startup and names every required setting that is missing.</summary>
        public void Validate()
        {
            var missing = new List<string>();
            if (ApiKey.Length == 0) missing.Add("OpenAI__ApiKey");
            if (Model.Length == 0) missing.Add("Chat__Model");

            if (missing.Count > 0)
                throw new InvalidOperationException("Missing settings: " + string.Join(", ", missing));
        }
    }
}
