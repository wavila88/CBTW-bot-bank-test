using System.Net.Http.Json;
using System.Text.Json;
using Domain.Exceptions;
using Domain.Models;
using Domain.Ports;

namespace Infra.Vector.Pinne
{
    /// <summary>
    /// Semantic search in Pinecone: embeds the question with the same model used at ingestion
    /// (with input_type=query) and asks the index for the closest fragments.
    /// </summary>
    public sealed class PineconeVector(HttpClient http, PineconeOptions options) : IVector
    {
        private sealed record IndexInfo(string Host);

        private string? _host;

        // Embeds the question, then asks the index for the closest guide fragments.
        public async Task<IReadOnlyList<VectorChunk>> SearchAsync(string query, int topK, CancellationToken ct = default)
        {
            var vector = await EmbedQueryAsync(query, ct);
            var host = await HostAsync(ct);

            var body = new
            {
                vector,
                topK,
                @namespace = options.Namespace,
                includeMetadata = true
            };
            using var response = await SendAsync(HttpMethod.Post, $"{host}/query", body, ct);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            return doc.RootElement.GetProperty("matches").EnumerateArray()
                .Select(match =>
                {
                    var metadata = match.TryGetProperty("metadata", out var m) ? m : default;
                    return new VectorChunk(
                        Id: match.GetProperty("id").GetString() ?? "",
                        Score: match.GetProperty("score").GetDouble(),
                        DocId: Text(metadata, "docId"),
                        Section: Text(metadata, "section"),
                        Type: Text(metadata, "type"),
                        Text: Text(metadata, "text"));
                })
                .OrderByDescending(c => c.Score)
                .ToList();
        }

        // Reads one metadata string; a missing or non-string field becomes empty.
        private static string Text(JsonElement metadata, string name) =>
            metadata.ValueKind == JsonValueKind.Object
            && metadata.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? ""
                : "";

        // Turns the question into a vector, marked as a query (not a document).
        private async Task<float[]> EmbedQueryAsync(string query, CancellationToken ct)
        {
            var body = new
            {
                model = options.EmbeddingModel,
                parameters = new { input_type = "query", truncate = "END", dimension = options.Dimension },
                inputs = new[] { new { text = query } }
            };
            using var response = await SendAsync(HttpMethod.Post, $"{options.ControlPlaneUrl}/embed", body, ct);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            return doc.RootElement.GetProperty("data")[0].GetProperty("values")
                .EnumerateArray().Select(v => v.GetSingle()).ToArray();
        }

        // Uses the configured host if there is one; otherwise looks it up once and keeps it.
        private async Task<string> HostAsync(CancellationToken ct)
        {
            if (_host is not null) return _host;

            if (!string.IsNullOrWhiteSpace(options.Host))
            {
                _host = WithScheme(options.Host);
                return _host;
            }

            var url = $"{options.ControlPlaneUrl}/indexes/{Uri.EscapeDataString(options.IndexName)}";
            using var response = await SendAsync(HttpMethod.Get, url, body: null, ct);

            var info = await response.Content.ReadFromJsonAsync<IndexInfo>(ct);
            _host = WithScheme(info!.Host);
            return _host;
        }

        private static string WithScheme(string host) =>
            host.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? host : "https://" + host;

        // Sends one call with the Pinecone headers; any failure becomes VectorUnavailableException.
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body, CancellationToken ct)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("Api-Key", options.ApiKey);
            request.Headers.Add("X-Pinecone-API-Version", options.ApiVersion);
            if (body is not null) request.Content = JsonContent.Create(body);

            HttpResponseMessage response;
            try
            {
                response = await http.SendAsync(request, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new VectorUnavailableException("The guide search service could not be reached.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode;
                response.Dispose();
                // Only the status code: the body may echo request details and must not reach the caller.
                throw new VectorUnavailableException($"The guide search service answered {status}.");
            }
            return response;
        }
    }
}
