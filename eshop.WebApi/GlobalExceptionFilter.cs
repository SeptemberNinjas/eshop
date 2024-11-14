using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eshop.WebApi
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GlobalExceptionFilter>();
        }

        public void OnException(ExceptionContext context)
        {
            context.Result = new ObjectResult(context.Exception.Message)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;

            _logger.LogError(context.Exception, "Необработанное исключение");
        }
    }
}
