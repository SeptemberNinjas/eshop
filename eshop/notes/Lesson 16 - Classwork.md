**План занятия:**
- Кэширование ответов от api
- Кэширование результатов запросов к СУБД (например, каталог) с использованием inMemoryCache (+возможно редис)
- Фоновая задача (BackgroundService) для удаления всех корзин

1. Добавим ResponseCache для услуг, т.к. в нашем приложении их список никогда не меняется.
    ```csharp
    //Program.cs
    ...
    builder.Services.AddResponseCaching(); // добавили сервис
    
    var app = builder.Build();
    
    DotNetRuntimeStatsBuilder.Default().StartCollecting();
    
    app.UseAuthentication();
    app.UseMiddleware<LogUserMiddleware>();
    
    app.UseSerilogRequestLogging(ops =>
    {
        ops.Logger = logger;
    });
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseMetricServer();
    app.UseResponseCaching(); // Добавили мидлвару
    app.UseAuthorization();
    
    app.MapControllers();
    
    app.Run();
    
    // CatalogController.cs
    ...
    [HttpGet("services")]
    [ResponseCache(VaryByQueryKeys = ["count"], Duration = 30)] // добавляем атрибут кэширования
    public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetServicesAsync([FromQuery]int? count, CancellationToken cancellationToken)
    ...
    ```
2. Запускаем проект демонстрируем что запрос первый раз идёт, второй раз нет. \
Тут нужно пояснить, что на самом деле мы тут ничего не кэшируем, а заставляем браузер кэшировать.
3. Для товаров, в качестве примера, добавим мемори кэш на уровне контроллера. Для этого добавим стандартный мемори кэш в DI `builder.Services.AddSingleton<IMemoryCache, MemoryCache>();`, заинжектим в контроллер и доработаем метод получения товаров.
   ```csharp
   [HttpGet("products")]
   public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetProductsAsync([FromQuery]int? count, CancellationToken cancellationToken)
   {
        var cacheKey = $"products_{count}";
        var result = await _memoryCache.GetOrCreateAsync(cacheKey, async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);
   
            var result = await _handler.GetItemsAsync(ItemTypes.Product, count, cancellationToken);
            if (result.IsFailed)
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1); // будем скидывать кэш, если получили fail
   
            return result;
        });
        
        if (result is null)
            return Problem();
        
        if (result.IsFailed)
            return BadRequest(result.ToString());
        
        if (!result.Value.Any())
            return NotFound();
   
        return Ok(result.Value);
   }
   ```
4. Запускаемся и демонстрируем кэширование товаров (точка остановка внутри лямбды и после GetOrCreateAsync)
5. Добавим кэширование на уровне репозитория. В качестве примера сделаем кэшируемый репозиторий с помощью декоратора. Для этого создадим класс `SaleItemCashedLinqToDbRepository`
   ```csharp
   // Добавить зависимость в eshop.Core
   <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="8.0.0" />
   
   // SaleItemCashedLinqToDbRepository.cs
   using eshop.Core;
   using eshop.Core.Cache;
   using Microsoft.Extensions.Caching.Memory;
   
   namespace eshop.DAL.LinqToDb
   {
       public class SaleItemCashedLinqToDbRepository : IReadOnlyRepository<SaleItem>
       {
           private const int RepositoryCacheLifetimeSeconds = 20;
   
           private readonly SaleItemLinqToDbRepository _repository;
           private readonly IMemoryCache _memoryCache;       
   
           public SaleItemCashedLinqToDbRepository(SaleItemLinqToDbRepository repository, IMemoryCache memoryCache)
           {
               _repository = repository;
               _memoryCache = memoryCache;           
           }
   
           public async Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
           {
               const string cacheKey = $"{nameof(SaleItemCashedLinqToDbRepository)}{nameof(GetAllAsync)}";
               return await WrapWithCacheAsync(cacheKey, () => _repository.GetAllAsync(cancellationToken));
           }
   
           public async Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
           {
               const string cacheKey = $"{nameof(SaleItemCashedLinqToDbRepository)}{nameof(GetByIdAsync)}";
               return await WrapWithCacheAsync(cacheKey, () => _repository.GetByIdAsync(id, cancellationToken));
           }
   
           public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
           {
               const string cacheKey = $"{nameof(SaleItemCashedLinqToDbRepository)}{nameof(GetCountAsync)}";
               return await WrapWithCacheAsync(cacheKey, () => _repository.GetCountAsync(cancellationToken));
           }
   
           private async Task<T> WrapWithCacheAsync<T>(string cacheKey, Func<Task<T>> func)
           {
               var result = await _memoryCache.GetOrCreateAsync(cacheKey,
                   async e =>
                   {
                       e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RepositoryCacheLifetimeSeconds);
                       
                       return await func();
                   });
   
               return result!;
           }
       }
   }
   ```
6. Изменим регистрацию репозитория
   ```csharp
   services.AddScoped<SaleItemLinqToDbRepository>();
   services.AddScoped<IReadOnlyRepository<SaleItem>, SaleItemCashedLinqToDbRepository>();
   ```
7. Запускаем и демонстрируем кэширование в репозитории.
8. Закэшированные в нашем приложении меняются только при создании заказа (пересчитываются остатки). Поэтому добавим возможность чистить кэш при необходимости \
Добавим класс хранилища ключей
   ```csharp
   // CacheKeysStorage.cs
   using System.Collections.Concurrent;
   using Microsoft.Extensions.Caching.Memory;
   
   namespace eshop.Core.Cache;
   
   public class CacheKeysStorage
   {
       private readonly ConcurrentDictionary<string, HashSet<string>> _groupKeys = new();
       private readonly IMemoryCache _memoryCache;
   
       public const string SaleItemsGroup = "SaleItemsGroup";
   
       public CacheKeysStorage(IMemoryCache memoryCache)
       {
           _memoryCache = memoryCache;
       }
   
       public void RegisterGroupKey(string groupName, string key)
       {
           _groupKeys.AddOrUpdate(
               groupName,
               _ => [key],
               (_, keys) =>
               {
                   keys.Add(key);
                   return keys;
               });
       }
   
       public void RemoveGroupCache(string groupName)
       {
           if (!_groupKeys.TryRemove(groupName, out var keys))
               return;
   
           foreach (var key in keys)
           {
               _memoryCache.Remove(key);
           }
       }
   }
   ```
9. Используем наше хранилище. Инжектим `CacheKeysStorage` через DI в классы ниже
   ```csharp
   // CatalogController.cs
   var result = await _memoryCache.GetOrCreateAsync(cacheKey, async e =>
   {
      e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);
   
      var result = await _handler.GetItemsAsync(ItemTypes.Product, count, cancellationToken);
      if (result.IsFailed)
          e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1);
   
      _keysStorage.RegisterGroupKey(CacheKeysStorage.SaleItemsGroup, cacheKey); // добавляем ключ в хранилище
      return result;
   });
   // SaleItemCashedLinqToDbRepository.cs
   private async Task<T> WrapWithCacheAsync<T>(string cacheKey, Func<Task<T>> func)
   {
      var result = await _memoryCache.GetOrCreateAsync(cacheKey,
          async e =>
          {
              e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RepositoryCacheLifetimeSeconds);
              _keysStorage.RegisterGroupKey(CacheKeysStorage.SaleItemsGroup, cacheKey); // добавляем ключ в хранилище
              return await func();
          });
   
      return result!;
   }
   // CreateOrderHandler.cs
   // после комита транзакции.
   _keysStorage.RemoveGroupCache(CacheKeysStorage.SaleItemsGroup); // чистим кэш
   ```
10. Ставим высокие значения для кэшей, добавляем товаров в корзину, создаем заказ. Проверяем, что данные загружаются не из кэша при обновлении главной страницы.
11. Добавим фоновый сервис для очистки корзин для чего сначала доработаем корзину, добавив поле с датой последнего изменения.
   ```csharp
   // Init.sql
   create table basket (
       id serial primary key,
       customer varchar(128) not null UNIQUE,
       last_update timestamp not null
   );
   // Basket.cs
   ...
   public DateTime LastUpdate { get; }
   ...
   public Basket(int id, IEnumerable<ItemsListLine> lines, string customer, DateTime lastUpdate)
   {
        Id = id;
        Customer = customer;
        LastUpdate = lastUpdate;
        _lines = lines.ToList();
   }
   ...
   // BasketRow.cs
   [Column("last_update"), NotNull]
   public DateTime LastUpdate { get; set; }
   
   // BasketLinqToDbRepository.cs (целиком)
   using eshop.Core;
   using eshop.DAL.LinqToDb.Schema;
   using LinqToDB;
   using LinqToDB.Data;
   
   namespace eshop.DAL.LinqToDb
   {
       public class BasketLinqToDbRepository : IRepository<Basket>
       {
           private readonly LinqToDbContext _context;
   
           public BasketLinqToDbRepository(LinqToDbContext context)
           {
               _context = context;
           }
   
           public async Task<IReadOnlyCollection<Basket>> GetAllAsync(CancellationToken cancellationToken = default)
           {
               return await _context.Baskets
                   .LoadWith(b => b.Lines)
                   .ThenLoad(l => l.Item)
                   .Select(r => MapBasket(r))
                   .ToListAsync(cancellationToken);
           }
   
           public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
           {
               return await _context.Baskets.CountAsync(cancellationToken);
           }
   
           public async Task<Basket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
           {
               var result = await _context.Baskets
                   .LoadWith(b => b.Lines)
                   .ThenLoad(l => l.Item)
                   .Where(r => r.Id == id)
                   .Select(r => MapBasket(r))
                   .ToListAsync(cancellationToken); // При наличии ассоциации один к многим метод FirstOrDefaultAsync не работает
   
               return result.FirstOrDefault();
           }
   
           public async Task UpdateAsync(Basket item, CancellationToken cancellationToken = default)
           {
               DataConnectionTransaction? transaction = null;
               if (_context.Transaction is null)
                   transaction = await _context.BeginTransactionAsync(cancellationToken);
   
               try
               {
                   await _context.Baskets
                       .Where(r => r.Id == item.Id)
                       .Set(r => r.Customer, item.Customer)
                       .Set(r => r.LastUpdate, DateTime.UtcNow) // добавили в апдейт
                       .UpdateAsync(cancellationToken);
   
                   await _context.BasketLines
                       .Where(r => r.BasketId == item.Id)
                       .DeleteAsync(cancellationToken);
   
                   var lines = item.Lines
                       .Select(l => new BasketLine
                       {
                           BasketId = item.Id,
                           ItemId = l.ItemId,
                           Count = l.Count
                       });
   
                   await _context.BasketLines.BulkCopyAsync(lines, cancellationToken);
   
                   if (transaction is not null)
                       await transaction.CommitAsync(cancellationToken);
               }
               catch
               {
                   if (transaction is not null)
                       await transaction.RollbackAsync(cancellationToken);
                   throw;
               }
               finally
               {
                   if (transaction is not null)
                       await transaction.DisposeAsync();
               }
           }
   
           public async Task<int> InsertAsync(Basket item, CancellationToken cancellationToken = default)
           {
               // Добавили в инсерт
               var id = await _context.Baskets.InsertWithIdentityAsync(() => new BasketRow { Customer = item.Customer, LastUpdate = DateTime.UtcNow }, cancellationToken);
               
              return (int)id;
           }
   
           private static ItemsListLine MapBasketLine(BasketLine line)
           {
               SaleItem item = (ItemTypes)line.Item!.Type switch
               {
                   ItemTypes.Product => new Product(line.Item.Id, line.Item.Name, line.Item.Price,
                       line.Item.Stock?.Amount ?? 0),
                   ItemTypes.Service => new Service(line.Item.Id, line.Item.Name, line.Item.Price),
                   _ => throw new ArgumentOutOfRangeException()
               };
   
               return new ItemsListLine(item, line.Count);
           }
   
           private static Basket MapBasket(BasketRow basket)
           {
               return new Basket(
                   basket.Id,
                   basket.Lines?.Select(MapBasketLine) ?? [],
                   basket.Customer!,
                   basket.LastUpdate);
           }
       }
   }
   ```
12. Добавим фоновый сервис
   ```csharp
   using eshop.Core;
   using eshop.DAL.Database;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Hosting;
   using Microsoft.Extensions.Logging;
   
   namespace eshop.Application.Order;
   
   public class ClearBasketsBackgroundService : BackgroundService
   {
       private const int DelayTimeoutMinutes = 1;
       private const int MaxBasketLifetimeHours = 1;
   
       private readonly ILogger<ClearBasketsBackgroundService> _logger;
       private readonly IServiceScopeFactory _serviceScopeFactory;
   
       public ClearBasketsBackgroundService(ILogger<ClearBasketsBackgroundService> logger,
           IServiceScopeFactory serviceScopeFactory)
       {
           _logger = logger;
           _serviceScopeFactory = serviceScopeFactory;
       }
   
       protected override async Task ExecuteAsync(CancellationToken cancellationToken)
       {
           while (!cancellationToken.IsCancellationRequested)
           {
               try
               {
                   await DoWorkAsync(cancellationToken);
               }
               catch (Exception e)
               {
                   _logger.LogError(e, "Ошибка при выполнении фоновой операции очистки корзин");
               }
   
               await Task.Delay(TimeSpan.FromMinutes(DelayTimeoutMinutes), cancellationToken);
           }
       }
   
       private async Task DoWorkAsync(CancellationToken cancellationToken)
       {
           using var scope = _serviceScopeFactory.CreateScope();
           var clearedBaskets = new List<Basket>();
           var repository = scope.ServiceProvider.GetRequiredService<IRepository<Basket>>();
           var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
           try
           {
               await context.BeginTransactionAsync(cancellationToken);
               var baskets = await repository.GetAllAsync(cancellationToken);
               foreach (var basket in baskets)
               {
                   if (basket.Lines.Count == 0 ||
                       DateTime.UtcNow - basket.LastUpdate < TimeSpan.FromHours(MaxBasketLifetimeHours))
                       continue;
                   basket.Clear();
                   await repository.UpdateAsync(basket, cancellationToken);
                   clearedBaskets.Add(basket);
               }
   
               await context.CommitTransactionAsync(cancellationToken);
               if (clearedBaskets.Count > 0)
                   _logger.LogInformation("Очищены корзины покупателей: {customers}",
                       string.Join(", ", clearedBaskets.Select(b => b.Customer)));
           }
           catch
           {
               await context.RollbackTransactionAsync(cancellationToken);
               throw;
           }
       }
   }
   ```
13. Зарегистрируем в DI 
   ```csharp
   builder.Services.AddHostedService<ClearBasketsBackgroundService>();
   ```
14. Перезапускаем compose, ставим приемлемое время жизни. Запускаем, создаем корзины, наполняем и смотрим как работает сервис. 