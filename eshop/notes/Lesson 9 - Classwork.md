**План занятия:**

Создать проект Asp.NET (сваггер, DI):
- контроллер для каталога
- контроллер для корзины
- контроллер для заказа
- Пример ДТО с атрибутами сериализации

1. Добавляем в решение новый проект `eshop.WebApi`. Убираем галку по HTTPS, ставим галки по OpenApi и Контроллерам.
2. Запускаем, показываем сваггер с пример погодного контроллера, затем вычищаем все типы про погоду.
3. Регистрацию хендлеров выносим из `ApplicationContext` в метод расширение 
```csharp
using eshop.Application.Order;
using eshop.Application.SaleItems;
using eshop.DAL;
using eshop.DAL.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eshop.Application;

public static class ConfigurationExtensions
{
    public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<RepositoryFactory>(_ => new DatabaseRepositoryFactory(configuration["ConnectionString"] ?? ""))
        // Регистрация обработчиков
        .AddScoped<GetSaleItemHandler>()
        .AddScoped<GetBasketHandler>()
        .AddScoped<CreateOrderHandler>()
        .AddScoped<AddBasketLineHandler>();
    
        return services;
    } 
}
```
4. Добавляем строку подключения в appsettings
```json
{
  "ConnectionString": "Host=localhost;Username=postgres;Password=password;Database=eshop"
}
```
5. Формируем `Program.cs` 
```csharp
using eshop.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterApplicationDependencies(builder.Configuration); // Подключаем нашу конфигурацию
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
```
6. Добавляем контроллер каталога `CatalogController.cs`
```csharp
using eshop.Application.SaleItems;
using eshop.Core;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CatalogController : ControllerBase
{
    private readonly GetSaleItemHandler _handler; // Зависимость только на хендлер

    public CatalogController(GetSaleItemHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("products")] // примеры гет запросов
    public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetProductsAsync(
        [FromQuery]int? count, // Пример атрибута с источником данных
        CancellationToken cancellationToken) // Токен обеспечивается системой
    {
        var result = await _handler.GetItemsAsync(ItemTypes.Product, count, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString()); // 400
        if (!result.Value.Any())
            return NotFound(); // 404

        return Ok(result.Value); // 200
    }
    
    [HttpGet("services")]
    public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetServicesAsync([FromQuery]int? count, CancellationToken cancellationToken)
    {        
        var result = await _handler.GetItemsAsync(ItemTypes.Service, count, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());
        if (!result.Value.Any())
            return NotFound();

        return Ok(result.Value);
    }
}
```
7. Добавляем контроллер корзины `BasketController.cs`
```csharp
using eshop.Application.Order;
using eshop.Core;
using eshop.WebApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;
 
[ApiController]
[Route("[controller]")]
public class BasketController : ControllerBase
{
    private readonly GetBasketHandler _getHandler;
    private readonly AddBasketLineHandler _addBasketLineHandler;

    public BasketController(GetBasketHandler getHandler, AddBasketLineHandler addBasketLineHandler)
    {
        _getHandler = getHandler;
        _addBasketLineHandler = addBasketLineHandler;
    }

    [HttpGet]
    public async Task<ActionResult<Basket>> GetBasketAsync(CancellationToken cancellationToken)
    {
        var result = await _getHandler.GetBasketAsync(cancellationToken);
        if (result.IsFailed)
            return NotFound();

        return Ok(result.Value);
    }
    
    [HttpPatch("line")] // пример патча
    public async Task<ActionResult<string>> AddLineAsync(
        [FromBody]AddLineRequest request, // Пример получения данных из body
        CancellationToken cancellationToken)
    {
        var result = await _addBasketLineHandler.AddLineAsync(request.ItemId, request.CountToAdd ?? 1, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());

        return Ok(result.ToString());
    }
}

// Requests/AddLineRequest.cs
public record AddLineRequest( // Пример модели запроса
    [property: JsonPropertyName("id")]int ItemId, // Использование атрибутов для маппинга полей json
    [property: JsonPropertyName("count")]int? CountToAdd);
```
8. Добавляем контроллер заказа `OrderController.cs`
```csharp
using eshop.Application.Order;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly CreateOrderHandler _createOrderHandler;

    public OrderController(CreateOrderHandler createOrderHandler)
    {
        _createOrderHandler = createOrderHandler;
    }
    
    [HttpPost] // Пример поста
    public async Task<ActionResult<string>> CreateOrderAsync(CancellationToken cancellationToken)
    {
        var result = await _createOrderHandler.CreateOrderAsync(cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());

        return Ok(result.ToString());
    }
}
```