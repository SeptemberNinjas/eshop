using Microsoft.AspNetCore.Mvc.Filters;

namespace eshop.WebApi
{
    public class IncomingRequestFilter : IAsyncActionFilter
    {
        private readonly ILogger<IncomingRequestFilter> _logger;

        public IncomingRequestFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IncomingRequestFilter>();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogDebug("{date:g}: route: {route}", DateTime.UtcNow, context.HttpContext.Request.Path);

            await next();
        }
    }
}
