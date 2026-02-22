using System.Diagnostics;
using System.Security.Claims;

namespace ExecutionOS.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        // Attach correlation ID to response headers for tracing
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        try
        {
            await _next(context);
            stopwatch.Stop();

            var userId = GetSafeUserId(context);

            _logger.LogInformation(
                "HTTP {Method} {Path} → {StatusCode} in {Duration}ms [User:{UserId}] [CID:{CorrelationId}]",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userId,
                correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "HTTP {Method} {Path} → 500 in {Duration}ms [CID:{CorrelationId}] Error: {ErrorMessage}",
                context.Request.Method,
                context.Request.Path.Value,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Extracts a safe, truncated user identifier — never logs full user ID or email.
    /// </summary>
    private static string GetSafeUserId(HttpContext context)
    {
        var claim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim)) return "anon";
        return claim.Length > 8 ? claim[..8] + "…" : claim;
    }
}
