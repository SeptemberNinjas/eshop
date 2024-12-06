namespace eshop.WebApi.Middlewares;

public class LogUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogUserMiddleware> _logger;

    public LogUserMiddleware(
        RequestDelegate next,
        ILogger<LogUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User.Identity?.Name;
        if (user is null)
        {
            await _next(context);
            return;
        }
            
        using var scope = _logger.BeginScope(new Dictionary<string, object> { { "UserName", user } });
        await _next(context);
    }
}