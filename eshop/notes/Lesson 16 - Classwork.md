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
3. Доба