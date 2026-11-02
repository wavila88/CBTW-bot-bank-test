using System.Text.Json;
using Api.Errors;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Api
{
    public class ApiExceptionHandlerTests
    {
        private const string InternalDetail = "pinecone-internal-detail-401";
        private const string Unavailable = "The search service is unavailable. Please try again.";

        [Theory]
        [InlineData(ErrorCode.InvalidRequest, 400, "The question is empty.")]
        [InlineData(ErrorCode.Unreachable, 503, Unavailable)]
        [InlineData(ErrorCode.Unavailable, 503, Unavailable)]
        [InlineData(ErrorCode.Rejected, 503, Unavailable)]
        [InlineData(ErrorCode.RateLimited, 503, Unavailable)]
        [InlineData(ErrorCode.NotFound, 503, Unavailable)]
        [InlineData(ErrorCode.Timeout, 504, "The search service took too long. Please try again.")]
        [InlineData(ErrorCode.Malformed, 502, "The search service returned an unexpected response.")]
        [InlineData(ErrorCode.TooManyRequests, 429, "You are sending requests too fast. Please wait a minute and try again.")]
        public async Task TryHandleAsync_ReturnsTheStatusAndCustomerMessageForEachCode(ErrorCode code, int status, string message)
        {
            var (context, body) = await HandleAsync(new DomainException(code, InternalDetail));

            Assert.Equal(status, context.Response.StatusCode);
            Assert.Equal(message, body.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task TryHandleAsync_UsesTheGenericMessageForAnyOtherException()
        {
            var (context, body) = await HandleAsync(new InvalidOperationException(InternalDetail));

            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("Something went wrong.", body.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task TryHandleAsync_NeverLeaksTheInternalDetail()
        {
            var (_, body) = await HandleAsync(new DomainException(ErrorCode.Rejected, InternalDetail, new Exception(InternalDetail)));

            Assert.DoesNotContain(InternalDetail, body.GetRawText());
        }

        [Fact]
        public async Task TryHandleAsync_ReturnsATraceIdAndLogsTheRealCause()
        {
            var logger = new Mock<ILogger<ApiExceptionHandler>>();
            var exception = new DomainException(ErrorCode.Rejected, InternalDetail);

            var (_, body) = await HandleAsync(exception, logger.Object);

            Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
            logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
        }

        private static async Task<(DefaultHttpContext Context, JsonElement Body)> HandleAsync(
            Exception exception, ILogger<ApiExceptionHandler>? logger = null)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var handler = new ApiExceptionHandler(logger ?? Mock.Of<ILogger<ApiExceptionHandler>>());

            await handler.TryHandleAsync(context, exception, CancellationToken.None);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = JsonDocument.Parse(context.Response.Body).RootElement.Clone();
            return (context, body);
        }
    }
}
