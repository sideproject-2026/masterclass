using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Lms.SharedKernel.Http;

/// <summary>
/// Last line of defence. Turns an unhandled exception into RFC 9457 ProblemDetails so no
/// endpoint can ever return a bare stack trace or an HTML error page.
/// </summary>
/// <remarks>
/// Reaching this handler means something went genuinely wrong: expected failures return
/// <c>Result</c> and never throw (artifacts/design/09-code-conventions.md §1). Every hit here
/// is logged at Error and is worth investigating.
/// </remarks>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Unhandled exception on {Method} {Path}. TraceId {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // The message is deliberately generic — exception text can carry connection strings,
        // file paths and SQL. The traceId is how a report gets correlated to the real log.
        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://lms.example.com/errors/unexpected",
                title = "An unexpected error occurred.",
                status = StatusCodes.Status500InternalServerError,
                detail = "The request could not be completed. Quote the traceId when reporting this.",
                traceId
            },
            cancellationToken);

        return true;
    }
}
