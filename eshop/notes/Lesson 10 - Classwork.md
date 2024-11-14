**План занятия:**

- Добавить контроллер для приема оплаты
- Добавить атрибуты для валидации модели входных данных контроллера оплаты
- Добавить фильтр для логирования входящих запросов
- Добавить фильтр для глобальной обработки ошибок
- Добавить аутентификацию через миддлвару

1. Реализуем хэндлер для совершения оплаты:

    Базоый хэндлер:

    ```csharp
        public abstract class PayOrderBaseHandler
        {
            private readonly RepositoryFactory _repositoryFactory;

            public PayOrderBaseHandler(RepositoryFactory repositoryFactory)
            {
                _repositoryFactory = repositoryFactory;
            }

            protected async Task<Result> PayAsync(int orderId, decimal amount, CancellationToken cancellationToken)
            {
                try
                {
                    var ordersRepository = _repositoryFactory.CreateOrdersRepository();

                    var order = await ordersRepository.GetByIdAsync(orderId, cancellationToken);

                    if (order == null)
                        return Result.Fail($"Заказ с идентификатором {orderId} не найден");

                    if (order.Status is not Core.OrderStatus.New)
                        return Result.Fail($"Заказ с идентификатором {orderId} нельзя оплатить");

                    var result = PaymentProcessing(order, amount);

                    if (result.IsFailed)
                        return Result.Fail("Не удалось выполнить оплату")
                            .WithErrors(result.Errors);

                    if (order.SetPaidStatus())
                    {
                        await ordersRepository.UpdateAsync(order, cancellationToken);
                    }

                    return Result.Ok()
                        .WithSuccess(result.Value);
                }
                catch (Exception ex)
                {
                    return Result.Fail("Не удалось оплатить заказ")
                        .WithError(ex.Message)
                        .WithError(ex.StackTrace);
                }
            }

            protected abstract Result<string> PaymentProcessing(Core.Order order, decimal amount);
        }
    ```

    Хэндлер для приема оплаты наличными:

    ```csharp
        public class PayOrderByCashHandler : PayOrderBaseHandler
        {    
            public PayOrderByCashHandler(RepositoryFactory repositoryFactory) : base(repositoryFactory)
            {
            }

            public async Task<Result> PayHandler(int orderId, decimal amount, CancellationToken cancellationToken)
            {
                return await PayAsync(orderId, amount, cancellationToken);
            }

            protected override Result<string> PaymentProcessing(Core.Order order, decimal amount)
            {
                var sum = order.Sum;

                if (amount < sum)
                    return Result.Fail("Недостаточно средств");

                if (amount == sum)
                    return Result.Ok("Заказ оплачен");

                return Result.Ok($"Заказ оплачен, сдача {amount - sum:F2}");
            }
        }
    ```

    Хэндлер для приема безналичной оплаты:

    ```csharp
        public class PayOrderByCashlessHandler : PayOrderBaseHandler
        {
            public PayOrderByCashlessHandler(RepositoryFactory repositoryFactory) : base(repositoryFactory)
            {
            }

            public async Task<Result> PayHandler(int orderId, decimal amount, CancellationToken cancellationToken)
            {
                return await PayAsync(orderId, amount, cancellationToken);
            }

            protected override Result<string> PaymentProcessing(Core.Order order, decimal amount)
            {
                var sum = order.Sum;

                if (amount < sum)
                    return Result.Fail("Недостаточно средств");

                if (amount != sum)
                    return Result.Fail($"Внесите ровно {sum}, при безналичном платеже переплата не допускается.");

                return Result.Ok("Заказ оплачен");
            }
        }
    ```

2. Добавляем новые хэндлеры в DI-контейнер:

    ```csharp

    ```

3. Реализуем контроллер оплаты

    ```csharp
        public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<RepositoryFactory>(_ => new DatabaseRepositoryFactory(configuration["ConnectionString"] ?? ""))
                // Регистрация обработчиков
                .AddScoped<ClearBasketHandler>()
                .AddScoped<GetOrdersHandler>()
                .AddScoped<GetSaleItemHandler>()
                .AddScoped<GetBasketHandler>()
                .AddScoped<CreateOrderHandler>()
                .AddScoped<AddBasketLineHandler>()
                .AddScoped<PayOrderByCashHandler>()
                .AddScoped<PayOrderByCashlessHandler>();
            
            return services;
        } 
    ```

4. Описываем модель входных данных для приема оплаты

    ```csharp
        public class PaymentRequest
        {
            [JsonPropertyName("orderId")]
            public int OrderId { get; set; }

            [Range(1, 2, ErrorMessage = "Тип оплаты должен быть: 1 (наличная оплата) или 2 (безналичная оплата)")]
            public byte PaymentType { get; set; }

            [JsonPropertyName("amount")]
            [Range(1, double.MaxValue, ErrorMessage = "Сумма для оплаты должны быть больше нуля")]
            public decimal Amount { get; set; }
        }
    ```

5. Реализуем новый контроллер

    ```csharp
        [Route("[controller]")]
        [ApiController]
        public class PaymentController : ControllerBase
        {
            private readonly PayOrderByCashHandler _payByCashHandler;
            private readonly PayOrderByCashlessHandler _payOrderByCashlessHandler;

            public PaymentController(
                PayOrderByCashHandler payByCashHandler,
                PayOrderByCashlessHandler payOrderByCashlessHandler)
            {
                _payByCashHandler = payByCashHandler;
                _payOrderByCashlessHandler = payOrderByCashlessHandler;
            }

            [HttpPatch]
            public async Task<ActionResult<string>> PayAsync(PaymentRequest request, CancellationToken cancellationToken)
            {
                var  result = request.PaymentType == 1 ?
                    await _payByCashHandler.PayHandler(request.OrderId, request.Amount, cancellationToken) :
                    await _payOrderByCashlessHandler.PayHandler(request.OrderId, request.Amount, cancellationToken);

                if (result.IsFailed)
                    return BadRequest(result.ToString());

                return Ok(result.ToString());
            }
        }
    ```

6. Приводим вывод ошибок валидации к тому же виду, что и ответы остальных контроллеров (`Program.cs`):

    ```csharp
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    string[] modelErrors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(x => x.Exception?.Message ?? x.ErrorMessage)
                        .ToArray();

                    var result = Result.Fail(modelErrors);
                    
                    return new BadRequestObjectResult(result.ToString());
                };
            });
    ```

7. Реализуем action-фильтр для логирования входящих запросов

    ```csharp
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
    ```

8. Добавляем новый фильтр в конфигурацию контрллеров (`Program.cs`):

    ```csharp
        builder.Services
            .AddControllers(options =>
            {
                options.Filters.Add<IncomingRequestFilter>();
            })
    ```

9. Реализуем exception-фильтр для глобальной обработки ошибок:

    Реализация фильтра:

    ```csharp
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
    ```

    Добавляем его в конфигурацию контроллеров:

    ```csharp
        builder.Services
            .AddControllers(options =>
            {
                options.Filters.Add<IncomingRequestFilter>();
                options.Filters.Add<GlobalExceptionFilter>();
            })
    ```

10. Реализуем middleware для проверки наличия ключа авторизации:

    ```csharp
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
    ```

