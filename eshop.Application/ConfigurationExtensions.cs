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
        services.AddScoped<RepositoryFactory>(_ => new DatabaseRepositoryFactory(configuration["ConnectionString"] ?? ""));
        services.AddScoped<GetSaleItemHandler>();
        return services;
    } 
}