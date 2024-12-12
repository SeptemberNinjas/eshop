using eshop.Application;
using eshop.Commands;
using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eshop;

/// <summary>
/// Контекст сеанса работы приложения
/// </summary>
public class ApplicationContext
{
    /// <summary>
    /// Описание приложения
    /// </summary>
    public const string Title = "Программа: 'Интернет магазин'";

    private readonly IServiceProvider _serviceProvider;

    public ApplicationContext(IConfiguration configuration)
    {
        var services = new ServiceCollection()
            .RegisterApplicationDependencies(configuration)
            // Регистрация команд
            .AddScoped<ClearBasketCommand>()
            .AddScoped<DisplayOrdersCommand>()
            .AddScoped<DisplayBasketCommand>()
            .AddScoped<AddBasketLineCommand>()
            .AddScoped<DisplayProductsCommand>()
            .AddScoped<CreateOrderCommand>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public IEshopCommand CreateCommand(CommandType commandType)
    {
        using var scope = _serviceProvider.CreateScope();
        
        return commandType switch
        {
            CommandType.Exit => new ExitCommand(),
            CommandType.Back => new BackCommand(),
            CommandType.GoToRoot => new GoToRootPageCommand(),
            CommandType.DisplaySaleItems => scope.ServiceProvider.GetRequiredService<DisplaySaleItemsCommand>(),
            CommandType.DisplayProducts => scope.ServiceProvider.GetRequiredService<DisplayProductsCommand>(),
            CommandType.DisplayServices => scope.ServiceProvider.GetRequiredService<DisplayServicesCommand>(),
            CommandType.DisplayBasket => scope.ServiceProvider.GetRequiredService<DisplayBasketCommand>(),
            CommandType.AddProductToBasket or 
            CommandType.AddServiceToBasket => scope.ServiceProvider.GetRequiredService<AddBasketLineCommand>(),
            CommandType.CreateOrder => scope.ServiceProvider.GetRequiredService<CreateOrderCommand>(),
            CommandType.DisplayOrders => scope.ServiceProvider.GetRequiredService<DisplayOrdersCommand>(),
            CommandType.StartOrderPayment => scope.ServiceProvider.GetRequiredService<StartOrderPaymentCommand>(),
            CommandType.SelectPaymentType => new SelectPaymentTypeCommand(),
            CommandType.TransferMoney => scope.ServiceProvider.GetRequiredService<TransferMoneyCommand>(),
            CommandType.ClearBasket => scope.ServiceProvider.GetRequiredService<ClearBasketCommand>(),
            _ => throw new NotSupportedException()
        };
    }

    public ICommandWithCommandsList GetInitialCommand()
    {
        return new InitialCommand(Title);
    }
}
