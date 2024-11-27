**План занятия:**

- Реализовать логирование с помощью Serilog
- Показать настройку Serilog через appsettings, с дополнительными фильтрами по уровню логирования
- Показать логирование в несколько провайдеров на примере логирования в файл и в консоль
- Показать прокидывание поля во все логи и альтернативное форматирование в файловом провайдере
- Показать запись метрик на примере prometheus (типовые и кастомные метрики)
- Показать работу с собранными метриками (поднять сервер prometheus и grafana), сделать простой дашборд

1. Запускаем установку образов которые нам потребуется, чтобы скачались пока делаем остальное или загружаем заранее. Для этого добавляем в файл `prometheus/parometheus.yaml` и запускаем обновленный `docker-compose.yaml`:
    ```yaml
    # prometheus/prometheus.yaml
    global:
      scrape_interval: 15s
      evaluation_interval: 15s
    
    scrape_configs:
      - job_name: eshop
        static_configs:
          - targets: ["host.docker.internal:5064"]
            
    # docker-compose.yaml
    version: '3.9'
    
    services:
      pg_eshop:
        image: postgres:17.0
        environment:
          POSTGRES_DB: "eshop"
          POSTGRES_PASSWORD: "password"
        ports:
          - "5432:5432"
        volumes:
          - pgdata:/data/postgres
          - ./init-scripts:/docker-entrypoint-initdb.d
            
      prometheus:
        image: prom/prometheus:latest
        volumes:
          - ./prometheus/:/etc/prometheus/
          - prometheus_data:/prometheus
        command:
          - '--config.file=/etc/prometheus/prometheus.yml'
        ports:
          - "9090:9090"
            
      grafana:
        image: grafana/grafana:latest
        depends_on:
          - prometheus
        ports:
          - "3000:3000"
        volumes:
          - grafana_data:/var/lib/grafana
          - ./grafana/provisioning/:/etc/grafana/provisioning/    
        
    volumes:
      pgdata:
      prometheus_data:
      grafana_data:
    ```
2. Подключаем Serilog. ссылки на пакеты для вставки в `eshop.WebApi.csproj`:
   ```xml
   <PackageReference Include="Serilog" Version="4.1.0" />
   <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
   ```
3. Инициализируем логгер в `Program.cs`:
   ```csharp
   var logger = new LoggerConfiguration()    // Создаём объект логгера
       .WriteTo.Console()    
       .CreateLogger();
   
   builder.Services.AddSerilog(logger); // Добавляем логгер в DI
   Log.Logger = logger; // Передаём объект в статический логгер
   ```
4. Запускаем проект. Видим, что форматирование логов изменилось, т.к. формат определяет теперь Serilog. Кроме того количество сообщений в логах сильно выросло, т.к. встроенные настройки логирования ASP теперь не учитываются и настройка `"Microsoft.AspNetCore": "Warning", "System": "Warning"` не работает.
5. Продемонстрируем настройку логгера через конфигурацию. Добавим в `appsettings.json` секцию:
   ```json
   {
     "Serilog": {    
       "MinimumLevel": {
         "Default": "Information",
         "Override": {
           "Microsoft.AspNetCore": "Warning",
           "System": "Warning"
         }
       }
     }
   }
   ```
6. Добавим применение настроек из конфигурации добавив в создании логгера:
   ```csharp
   var logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .WriteTo.Console()
       .CreateLogger();
   ```
7. Запустим проект. Видим, что логи теперь фильтруются. Добавим мидвару логирования запросов от Serilog `app.UseSerilogRequestLogging();` сразу после `var app = builder.Build();`. Теперь при запуске проекта должны появиться дополнительные логи запросов.
8. Продемонстрируем перезапись (override) настроек. Для этого сначала сделаем лог в нашем фильтре уровнем debug. После запуска лог исчезнет.
   ```csharp
   // IncomingRequestFilter.cs
   _logger.LogDebug("{date:g}: route: {route}", DateTime.UtcNow, context.HttpContext.Request.Path);
   ```
9. Добавим снижение уровня для нашего фильтра до Debug. После запуска логи должны снова появиться.
   ```json
   {
     "Serilog": {    
       "MinimumLevel": {
         "Default": "Information",
         "Override": {
           "Microsoft.AspNetCore": "Warning",
           "System": "Warning",
           "eshop.WebApi.IncomingRequestFilter": "Debug" 
         }
       }
     }
   }
   ```
10. Добавим второй провайдер логирования в файл. После запуска показываем, что создался файл и логи пишутся и в него.
   ```csharp
   var logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .WriteTo.Console()
       .WriteTo.File("logs.txt")
       .CreateLogger();
   ```
11. Поменяем форматирование для логов в файле. После запуска показываем, что логи в файле теперь пишутся в json формате и включают дополнительные поля.
   ```csharp
   var logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .WriteTo.Console()
       .WriteTo.File(new CompactJsonFormatter(), "logs.txt")
       .CreateLogger();
   ```
12. Продемонстрируем добавление поля во все логи. После запуска показываем наличие этого поля в логах в файле.
   ```csharp
   var logger = new LoggerConfiguration()
       .Enrich.WithProperty("ApplicationName", "eshop")
       .ReadFrom.Configuration(builder.Configuration)
       .WriteTo.Console()
       .WriteTo.File(new CompactJsonFormatter(), "logs.txt")
       .CreateLogger();
   ```
13. Продемонстрируем разный уровень логирования в разных провайдерах. После запуска показываем, что в файле остались debug логи, а в консоли их нет.
   ```csharp
   var logger = new LoggerConfiguration()
       .Enrich.WithProperty("ApplicationName", "eshop")
       .ReadFrom.Configuration(builder.Configuration)
       .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
       .WriteTo.File(new CompactJsonFormatter(), "logs.txt")
       .CreateLogger();
   ```
14. Демонстрируем работу с метриками. Запускаем наш docker-compose. Запускаем проект, наблюдаем обращения по эндпоинту /metrics каждые 15 сек.
15. Добавляем в проект нугет пакеты:
   ```xml
   <PackageReference Include="prometheus-net" Version="8.2.1" />
   <PackageReference Include="prometheus-net.AspNetCore" Version="8.2.1" />
   <PackageReference Include="prometheus-net.DotNetRuntime" Version="4.4.1" />
   ```
16. Добавляем эндпоинт для метрик. После запуска демонстрируем появление дефолтных метрик.
   ```csharp
   app.UseMetricServer();
   app.UseAuthorization(); // перед этой строкой
   ```
17. Добавляем доп. метрики библиотеки `prometheus-net.DotNetRuntime`. После запуска демонстрируем появление дополнительных метрик.
   ```csharp
   DotNetRuntimeStatsBuilder.Default().StartCollecting();
   ```
18. Добавляем свою "бизнесовую" метрику. Пусть это будет количество не пустых корзин. Для удобства интегрируем её в репозиторий корзины. 
   ```csharp
    private readonly Gauge _notEmptyBaskets; 
    
    public BasketDatabaseRepository(string connectionString) : base(connectionString)
    {
        _notEmptyBaskets = Metrics.CreateGauge("eshop_not_empty_baskets", "Количество не пустых корзин");
    }
    ...
    public async Task<IReadOnlyCollection<Basket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var commandText = 
            $"""
             select b.id as basket_id, b.customer, c.id as item_id, c.price, c."name", c."type" , s.amount , bl.count
             from basket b
             left join basket_line bl on b.id = bl.basket_id
             left join catalog c on bl.item_id = c.id
             left join stock s on c.id = s.id
             """;

        var result = await ExecuteReaderListAsync(commandText, GetBasketLine, cancellationToken);
        var baskets = result
            .GroupBy(r => r.Id)
            .Select(g => new Basket(g.Key, g
                .Where(i => i.Item is not null)
                .Select(i => i.Item!), g.First().Customer))
            .ToArray();
        
        _notEmptyBaskets.Set(baskets.Count(b => b.Lines.Count != 0));

        return baskets;
    }
   ```
19. Запускаем проект, создаем и наполняем корзины несколькими пользователями. Демонстрируем наличие метрики на нашем эндпоинте.
20. Демонстрируем работу с собираемыми метриками на примере grafana (уже запущена). Заходим в графану `http://localhost:3000`. По умолчанию admin/admin. В настройках источников проверяем есть ли коннект к прому, он должен быть доступен по адресу `http://host.docker.internal:9090`.\
Создаём небольшой дашборд с нашей метрикой `eshop_not_empty_baskets` или какой либо другой метрикой из доступных.