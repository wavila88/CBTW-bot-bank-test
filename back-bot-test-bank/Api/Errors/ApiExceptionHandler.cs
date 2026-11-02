using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors
{
    /// <summary>Turns any exception into a generic JSON response. The real cause goes only to the server log.</summary>
    public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            var code = exception is DomainException domain ? (ErrorCode?)domain.Code : null;
            var (status, message) = Describe(code);

            logger.LogError(exception, "Request failed with {Code}, trace {TraceId}", code?.ToString() ?? "Unexpected", context.TraceIdentifier);

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                status,
                title = "Request failed",
                detail = message,
                traceId = context.TraceIdentifier
            }, ct);
            return true;
        }

        // The only place where error codes become HTTP statuses and customer messages.
        private static (int Status, string Message) Describe(ErrorCode? code) => code switch
        {
            ErrorCode.InvalidRequest => (400, "The question is empty."),
            ErrorCode.Timeout => (504, "The search service took too long. Please try again."),
            ErrorCode.Malformed => (502, "The search service returned an unexpected response."),
            ErrorCode.TooManyRequests => (429, "You are sending requests too fast. Please wait a minute and try again."),
            null => (500, "Something went wrong."),
            _ => (503, "The search service is unavailable. Please try again.")
        };
    }
}
