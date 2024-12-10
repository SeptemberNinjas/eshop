**План занятия:**

- Внедрить транзакционное выполнение команды добавления строки в корзину
- Продемострировать явную и неявную передачу контекста
- Подключить ORM (linq2db)

1. Подготавливаем текущий контекст для БД - `DatabaseContext` к внедрению транзакций:

    - Делаем его публичным
    - Удаляем синхронный метод создания подключения
    - Делаем асинхронным метод создания команды к БД

2. Создаем базовый класс для репозиториев - `BaseRepository`:

    ```csharp
      public abstract class BaseRepository
      {
          private readonly DatabaseContext _databaseContext;

          public BaseRepository(DatabaseContext databaseContext)
          {
              _databaseContext = databaseContext;
          }

          public NpgsqlCommand GetCommand(string text)
          {
              return Task.Run(async () => await _databaseContext.GetCommandAsync(text)).Result;
          }

          public async Task<List<T>> ExecuteReaderListAsync<T>(string commandText, Func<DbDataReader, T> binging, CancellationToken cancellationToken)
          {
              var command = await _databaseContext.GetCommandAsync(commandText);
              using var reader = await command.ExecuteReaderAsync(cancellationToken);

              var result = new List<T>();

              while (await reader.ReadAsync(cancellationToken))
              {
                  result.Add(binging(reader));
              }

              return result;
          }

          public async Task<T?> ExecuteReaderAsync<T>(string commandText, Func<DbDataReader, T> binding, CancellationToken cancellationToken)
          {
              var command = await _databaseContext.GetCommandAsync(commandText);

              using var reader = await command.ExecuteReaderAsync(cancellationToken);

              if (await reader.ReadAsync(cancellationToken))
                  return binding(reader);


              return default;
          }

          public async Task<string?> ExecuteScalarAsync(string commandText, CancellationToken cancellationToken)
          {
              var command = await _databaseContext.GetCommandAsync(commandText);

              var reader = await command.ExecuteScalarAsync(cancellationToken);
              return reader?.ToString();
          }
      }
    ```

3. Создаем методы работы с транзакциями в `DatabaseContext`:

    ```csharp

      private NpgsqlTransaction? _transaction;

      public async Task BeginTransactionAsync()
      {
          var connection = await GetConnectionAsync();

          _transaction = await connection.BeginTransactionAsync();
      }

      public async Task CommitTransactionAsync()
      {
          if (_transaction != null)
              await _transaction.CommitAsync();
      }
    ```

4. Наследуем от `BaseRepository` (вместо `DatabaseContext`) все существующие репозитории

5. Актуализируем создание репозиториев в `DatabaseRepositoryFactory`:

    Контекст к БД будем получать из DI-контейнера

    ```csharp
      public class DatabaseRepositoryFactory : RepositoryFactory
      {
          private DatabaseContext _databaseContext;

          public DatabaseRepositoryFactory(DatabaseContext databaseContext)
          {
              _databaseContext = databaseContext;
          }

          public override IRepository<Basket> CreateBasketRepository()
          {
              return new BasketDatabaseRepository(_databaseContext);
          }

          public override IRepository<Order> CreateOrdersRepository()
          {
              return new OrderDatabaseRepository(_databaseContext);
          }

          /// <inheritdoc/>
          public override IReadOnlyRepository<SaleItem> CreateSaleItemRepository()
          {
              return new SaleItemDatabaseRepository(_databaseContext);
          }

          /// <inheritdoc/>
          public override IRepository<Stock> CreateStockRepository()
          {
              return new StockDatabaseRepository(_databaseContext);
          }
      }
    ```

6. Добавляем `DatabaseContext` в DI-контейнер:

    ```csharp
      public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
      {
          services
              .AddScoped(_ => new DatabaseContext(configuration["ConnectionString"] ?? ""))
              .AddScoped<RepositoryFactory,DatabaseRepositoryFactory>()
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

7. Правим хэндлер `AddBasketLineHandler`:

    - Получаем `DatabaseContext` из DI-контейнера
    - Добавляем явное открытие и коммит транзакции
    - Создаем репозитории самостоятельно с явной передачей контекста, вместо использования фабрики

    ```csharp
      public class AddBasketLineHandler
      {
          private readonly DatabaseContext _databaseContext;

          public AddBasketLineHandler(DatabaseContext databaseContext)
          {
              _databaseContext = databaseContext;
          }

          public async Task<Result> AddLineAsync(string customer, int itemId, int count,
    CancellationToken cancellationToken)
          {
              try
              {
                  await _databaseContext.BeginTransactionAsync();
                  
                  var basketRepository = new BasketDatabaseRepository(_databaseContext);
                  
                  var baskets = await basketRepository.GetAllAsync(cancellationToken);
                  var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
                  if (customerBasket is null)
                  {
                      await basketRepository.InsertAsync(new Basket(customer), cancellationToken);
                      customerBasket = (await basketRepository.GetAllAsync(cancellationToken))
                          .FirstOrDefault(b => b.Customer == customer);
                  }
                  if (customerBasket is null)
                      return Result.Fail("Корзина не найдена");

                  var itemsRepository = new SaleItemDatabaseRepository(_databaseContext);
                  var item = await itemsRepository.GetByIdAsync(itemId, cancellationToken);
                  if (item is null)
                      return Result.Fail("Товар или услуга не найдены");
            
                  var result = item switch
                  {
                      Product product => await AddLineAsync(product, count, customerBasket, basketRepository),
                      Service service => AddLine(service, customerBasket),
                      _ => Result.Fail("Неизвестный тип товарной единицы")
                  };

                  if (result.IsSuccess)
                      await basketRepository.UpdateAsync(result.Value, cancellationToken);

                  await _databaseContext.CommitTransactionAsync();

                  return result.ToResult();
              }
              catch (Exception ex)
              {
                  return Result.Fail("Не удалось получить корзину")
                      .WithError(ex.Message)
                      .WithError(ex.StackTrace);
              }
          }
      }
    ```

8. Правим хэндлер `CreateOrderHandler` для неявной передачи контекста и открытой транзакции:

    ```csharp
      using eshop.Core;
      using eshop.DAL;
      using eshop.DAL.Database;
      using FluentResults;

      namespace eshop.Application.Order;

      public class CreateOrderHandler
      {
          private readonly DatabaseContext _databaseContext;
          private readonly RepositoryFactory _repositoryFactory;

          public CreateOrderHandler(
              DatabaseContext databaseContext,
              RepositoryFactory repositoryFactory)
          {
              _databaseContext = databaseContext;
              _repositoryFactory = repositoryFactory;
          }

          public async Task<Result> CreateOrderAsync(CancellationToken cancellationToken)
          {
              try
              {
                  await _databaseContext.BeginTransactionAsync();

                  var basketRepository = _repositoryFactory.CreateBasketRepository();
                  var currentBasket = (await basketRepository.GetAllAsync(cancellationToken)).FirstOrDefault();
                  if (currentBasket is null || currentBasket.Lines.Count == 0)
                      return Result.Fail("Корзина не найдена");
            
                  var order = currentBasket.CreateOrderFromBasket();
                  if (order is null)
                      return Result.Fail("Ошибка при создании заказа. Корзина пуста");

                  var stockRepository = _repositoryFactory.CreateStockRepository();
                  var orderedProductsWithCount = order.Lines
                      .Where(l => l.ItemType is ItemTypes.Product)
                      .Join(await stockRepository.GetAllAsync(cancellationToken),
                          orderLine => orderLine.ItemId, 
                          stock => stock.ItemId,
                          (orderLine, stock) => (stock, orderLine.Count));
                      
                  // Тут происходят взаимосвязанные изменения в трёх репозиториях
                  // Хорошее место, чтобы показать транзакцию
                  var ordersRepository = _repositoryFactory.CreateOrdersRepository();
                  var id = await ordersRepository.InsertAsync(order, cancellationToken);
                  await basketRepository.UpdateAsync(currentBasket, cancellationToken);
                  foreach (var (stock, count) in orderedProductsWithCount)
                  {
                      if (stock.Amount - count < 0)
                          return Result.Fail("Недостаточно товара");

                      stock.Amount -= count;
                      await stockRepository.UpdateAsync(stock, cancellationToken);
                  }

                  await _databaseContext.CommitTransactionAsync();

                  return Result.Ok()
                      .WithSuccess($"Создан заказ {id}");
              }
              catch (Exception ex)
              {
                  return Result.Fail("Не удалось создать заказ")
                      .WithError(ex.Message)
                      .WithError(ex.StackTrace);
              }
          }
      }
    ```

9. Подключаем библиотеку `linq2db` в проекте `eshop.DAL`

10. Описываем схемы данных для таблиц `catalog` и `stock` (в проекте `eshop.DAL`):

    ```csharp
      [Table(Name = "catalog")]
      public class Catalog
      {
          [Column("id")]
          public int Id { get; set; }

          [Column("name")]
          public string Name { get; set; } = null!;

          [Column("price")]
          public decimal Price { get; set; }

          [Column("type")]
          public short Type { get; set; }
      }
    ```

    ```csharp
    [Table(Name = "stock")]
    public class Stock
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("amount")]
        public int Amount { get; set; }
    }
    ```

11. Описываем контекст взаимодействия с БД через `linq2db` (в проекте `eshop.DAL`):

    ```csharp
      public class LinqToDbContext : DataConnection
      {
          public ITable<Catalog> Catalog => this.GetTable<Catalog>();

          public ITable<Stock> Stock => this.GetTable<Stock>();

          public LinqToDbContext(DataOptions options) : base(options) { }
      }
    ```

12. Пишем новую реализацию для репозитрия `IReadOnlyRepository<SaleItem>`, которая будет работать через ORM:

    ```csharp
      public class SaleItemLinqToDbRepository : IReadOnlyRepository<SaleItem>
      {
          private readonly LinqToDbContext _context;

          public SaleItemLinqToDbRepository(LinqToDbContext context)
          {
              _context = context;
          }

          private static SaleItem GetSaleItem(Catalog catalog, LinqToDb.Schema.Stock stock)
          {
              var type = (ItemTypes)catalog.Type;

              return type switch
              {
                  ItemTypes.Product => new Product(catalog.Id, catalog.Name, catalog.Price, stock.Amount),
                  ItemTypes.Service => new Service(catalog.Id, catalog.Name, catalog.Price),
                  _ => throw new ArgumentOutOfRangeException()
              };
          }

          public async Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
          {
              var result = await _context.Catalog
                  .LeftJoin(_context.Stock.AsQueryable(),
                      (c, s) => c.Id == s.Id,
                      (c, s) => GetSaleItem(c, s))
                  .ToListAsync(cancellationToken);

              return result;
          }

          public async Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
          {
              var result = await _context.Catalog
                  .Where(c => c.Id == id)
                  .LeftJoin(_context.Stock.AsQueryable(),
                      (c, s) => c.Id == s.Id,
                      (c, s) => GetSaleItem(c, s))
              .FirstOrDefaultAsync(cancellationToken);

              return result;
          }

          public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
          {
              return await _context.Catalog.CountAsync(cancellationToken);
          }
      }
    ```

13. Подключаем ORM и добавляем новый репозитрий в DI-контейнер:

    ```csharp
      public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
      {
          services
              .AddScoped(_ => new DatabaseContext(configuration["ConnectionString"] ?? ""))
              .AddScoped<RepositoryFactory,DatabaseRepositoryFactory>()
              // Регистрация обработчиков
              .AddScoped<ClearBasketHandler>()
              .AddScoped<GetOrdersHandler>()
              .AddScoped<GetSaleItemHandler>()
              .AddScoped<GetBasketHandler>()
              .AddScoped<CreateOrderHandler>()
              .AddScoped<AddBasketLineHandler>()
              .AddScoped<PayOrderByCashHandler>()
              .AddScoped<PayOrderByCashlessHandler>();

          services.AddLinqToDBContext<LinqToDbContext>((sp, options) =>
          {
              var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

              return options
                  .UsePostgreSQL(configuration["ConnectionString"] ?? "")
                  .UseLoggerFactory(loggerFactory)
                  .UseTraceLevel(System.Diagnostics.TraceLevel.Verbose);
          });

          services.AddScoped<IReadOnlyRepository<SaleItem>, SaleItemLinqToDbRepository>();

          return services;
      }
    ```

14. Меняем в хэндлере `GetSaleItemHandler` реализацию репозитрия:

    ```csharp
      public class GetSaleItemHandler
      {
          private readonly IReadOnlyRepository<SaleItem> _saleItemRepository;

          public GetSaleItemHandler(IReadOnlyRepository<SaleItem> saleItemRepository)
          {
              _saleItemRepository = saleItemRepository;
          }

          public async Task<Result<IEnumerable<SaleItemDto>>> GetItemsAsync(ItemTypes itemType, int? count, CancellationToken cancellationToken)
          {
              try
              {
                  var items = (await _saleItemRepository
                      .GetAllAsync(cancellationToken))
                      .Where(i => i.ItemType == itemType);

                  var requestedItems = count is null or <= 0
                      ? items
                      : items.Take(count.Value);

                  return Result.Ok(requestedItems
                      .Select(i => new SaleItemDto(i.ItemType, i.Id, i.Name, i.Price, (i as Product)?.Stock)));
              }
              catch (Exception ex)
              {
                  return Result.Fail("Не удалось получить коллекцию торговых единиц")
                      .WithError(ex.Message)
                      .WithError(ex.StackTrace);
              }
          }
      }
    ```