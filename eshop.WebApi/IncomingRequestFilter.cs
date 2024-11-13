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
            _logger.LogInformation($"{DateTime.UtcNow:g}: route: {context.HttpContext.Request.Path}");

            await next();
        }
    }
}
