using eshop.Application.Order;
using eshop.Application.Payment;
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
}