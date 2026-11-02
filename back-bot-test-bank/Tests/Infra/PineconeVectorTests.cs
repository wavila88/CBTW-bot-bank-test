using System.Text.Json;
using Domain.Exceptions;
using Infra.Vector.Pinne;

namespace Tests.Infra
{
    public class PineconeVectorTests
    {
        private const string IndexUrl = "https://api.pinecone.io/indexes/customer-service-kb";
        private const string EmbedUrl = "https://api.pinecone.io/embed";
        private const string QueryUrl = "https://idx.svc.pinecone.io/query";

        private static readonly PineconeOptions Options = new()
        {
            ApiKey = "test-key",
            IndexName = "customer-service-kb",
            Namespace = "kb",
            EmbeddingModel = "llama-text-embed-v2",
            Dimension = 1024
        };

        // Matches come unsorted on purpose, and the last one has no metadata at all.
        private const string QueryResponse = """
            {"matches":[
              {"id":"KB-03-q1","score":0.5,"metadata":{"docId":"KB-03","section":"FAQ","type":"faq","text":"Q: ..."}},
              {"id":"KB-01-s2","score":0.8123,"metadata":{"docId":"KB-01","section":"Checklist","type":"guide","text":"Card > Limits"}},
              {"id":"X-1","score":0.2}
            ]}
            """;

        private static StubHandler Handler() => new(request => request.RequestUri!.ToString() switch
        {
            IndexUrl => StubHandler.Ok("{\"host\":\"idx.svc.pinecone.io\"}"),
            EmbedUrl => StubHandler.Ok("{\"data\":[{\"values\":[0.1,0.2,0.3]}]}"),
            QueryUrl => StubHandler.Ok(QueryResponse),
            var url => StubHandler.NotFound(url)
        });

        private static PineconeVector CreateVector(StubHandler handler) => new(new HttpClient(handler), Options);

        [Fact]
        public async Task SearchAsync_ReturnsTheBestMatchFirst()
        {
            var chunks = await CreateVector(Handler()).SearchAsync("hello", 5);

            Assert.Equal(new[] { "KB-01-s2", "KB-03-q1", "X-1" }, chunks.Select(c => c.Id));
        }

        [Fact]
        public async Task SearchAsync_MapsTheGuideMetadata()
        {
            var best = (await CreateVector(Handler()).SearchAsync("hello", 5))[0];

            Assert.Equal("KB-01", best.DocId);
            Assert.Equal("Checklist", best.Section);
            Assert.Equal("guide", best.Type);
            Assert.Equal("Card > Limits", best.Text);
            Assert.Equal(0.8123, best.Score, 4);
        }

        [Fact]
        public async Task SearchAsync_ToleratesAMatchWithoutMetadata()
        {
            var chunks = await CreateVector(Handler()).SearchAsync("hello", 5);

            var withoutMetadata = chunks.Single(c => c.Id == "X-1");
            Assert.Equal("", withoutMetadata.DocId);
            Assert.Equal("", withoutMetadata.Text);
        }

        // A document-type embedding here would silently make the search worse, so it is worth pinning down.
        [Fact]
        public async Task SearchAsync_EmbedsTheQuestionAsAQuery()
        {
            var handler = Handler();

            await CreateVector(handler).SearchAsync("my card was declined", 5);

            var embed = JsonDocument.Parse(handler.Requests.Single(r => r.Url == EmbedUrl).Body!).RootElement;
            Assert.Equal("query", embed.GetProperty("parameters").GetProperty("input_type").GetString());
            Assert.Equal("my card was declined", embed.GetProperty("inputs")[0].GetProperty("text").GetString());
        }

        // The port promises a Domain error, so Application never has to know about HTTP.
        [Fact]
        public async Task SearchAsync_ReportsAFailedCallAsVectorUnavailable()
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized));

            await Assert.ThrowsAsync<VectorUnavailableException>(() => CreateVector(handler).SearchAsync("hello", 5));
        }

        [Fact]
        public async Task SearchAsync_UsesTheConfiguredHostWithoutLookingItUp()
        {
            var handler = Handler();
            var options = new PineconeOptions { ApiKey = "test-key", Host = "idx.svc.pinecone.io" };

            await new PineconeVector(new HttpClient(handler), options).SearchAsync("hello", 5);

            Assert.DoesNotContain(handler.Requests, r => r.Url == IndexUrl);
            Assert.Single(handler.Requests, r => r.Url == QueryUrl);
        }

        [Fact]
        public async Task SearchAsync_LooksUpTheIndexHostOnlyOnce()
        {
            var handler = Handler();
            var vector = CreateVector(handler);

            await vector.SearchAsync("one", 5);
            await vector.SearchAsync("two", 5);

            Assert.Single(handler.Requests, r => r.Url == IndexUrl);
        }
    }
}
