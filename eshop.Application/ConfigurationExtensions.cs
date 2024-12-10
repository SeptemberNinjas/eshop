using eshop.Application.Order;
using eshop.Application.Payment;
using eshop.Application.SaleItems;
using eshop.Core;
using eshop.DAL;
using eshop.DAL.Database;
using eshop.DAL.LinqToDb;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eshop.Application;

public static class ConfigurationExtensions
{
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
}