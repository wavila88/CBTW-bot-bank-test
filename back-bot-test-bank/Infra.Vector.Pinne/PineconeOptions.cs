namespace Infra.Vector.Pinne
{
    public sealed class PineconeOptions
    {
        // Set through environment variables (Pinecone__ApiKey, ...) or appsettings.Local.json. No defaults in code.
        public string ApiKey { get; set; } = "";
        public string IndexName { get; set; } = "";
        public string Namespace { get; set; } = "";

        // Optional. Index host (idx-xxxx.svc.pinecone.io). When empty, it is looked up once.
        public string Host { get; set; } = "";

        // Must match the model and dimension used at ingestion.
        public string EmbeddingModel { get; set; } = "";
        public int Dimension { get; set; }

        // Pinecone protocol constants, not environment decisions.
        public string ApiVersion { get; set; } = "2025-04";
        public string ControlPlaneUrl { get; set; } = "https://api.pinecone.io";

        /// <summary>Fails at startup and names every required setting that is missing.</summary>
        public void Validate()
        {
            var missing = new List<string>();
            if (ApiKey.Length == 0) missing.Add("Pinecone__ApiKey");
            if (IndexName.Length == 0) missing.Add("Pinecone__IndexName");
            if (Namespace.Length == 0) missing.Add("Pinecone__Namespace");
            if (EmbeddingModel.Length == 0) missing.Add("Pinecone__EmbeddingModel");
            if (Dimension <= 0) missing.Add("Pinecone__Dimension");

            if (missing.Count > 0)
                throw new InvalidOperationException("Missing settings: " + string.Join(", ", missing));
        }
    }
}
