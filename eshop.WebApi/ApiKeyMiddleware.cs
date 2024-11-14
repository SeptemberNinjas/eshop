namespace eshop.WebApi
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyMiddleware(
            RequestDelegate next,
            ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var apiKey = context.Request.Headers.Authorization;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("API ключ не был предоставлен.");

                return;
            }

            await _next(context);
        }
    }
}
