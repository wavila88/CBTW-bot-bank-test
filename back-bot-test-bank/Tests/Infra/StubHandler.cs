using System.Net;
using System.Text;

namespace Tests.Infra
{
    /// <summary>Fake HTTP handler: records every request and answers with the given function, so adapters are tested without network.</summary>
    public sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(string Url, string? Body)> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            Requests.Add((request.RequestUri!.ToString(), body));
            return respond(request);
        }

        public static HttpResponseMessage Ok(string json) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        public static HttpResponseMessage NotFound(string url) => new(HttpStatusCode.NotFound)
        {
            Content = new StringContent("unexpected route: " + url)
        };
    }
}
