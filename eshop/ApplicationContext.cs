using eshop.Application;
using eshop.Application.Order;
using eshop.Application.SaleItems;
using eshop.Commands;
using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using eshop.DAL;
using eshop.DAL.Database;
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
            .AddScoped<DisplayBasketCommand>()
            .AddScoped<AddBasketLineCommand>()
            .AddScoped<DisplayProductsCommand>()
            .AddScoped<CreateOrderCommand>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public IEshopCommand CreateCommand(CommandType commandType)
    {
        using var scope = _serviceProvider.CreateScope();
        var repositoryFactory = scope.ServiceProvider.GetRequiredService<RepositoryFactory>();
        var getSaleItemHandler = scope.ServiceProvider.GetRequiredService<GetSaleItemHandler>();

        return commandType switch
        {
            CommandType.Exit => new ExitCommand(),
            CommandType.Back => new BackCommand(),
            CommandType.GoToRoot => new GoToRootPageCommand(),
            CommandType.DisplaySaleItems => new DisplaySaleItemsCommand(),
            CommandType.DisplayProducts => scope.ServiceProvider.GetRequiredService<DisplayProductsCommand>(),
            CommandType.DisplayServices => scope.ServiceProvider.GetRequiredService<DisplayServicesCommand>(),
            CommandType.DisplayBasket => scope.ServiceProvider.GetRequiredService<DisplayBasketCommand>(),
            CommandType.AddProductToBasket or 
            CommandType.AddServiceToBasket => scope.ServiceProvider.GetRequiredService<AddBasketLineCommand>(),
            CommandType.CreateOrder => scope.ServiceProvider.GetRequiredService<CreateOrderCommand>(),
            CommandType.DisplayOrders => new DisplayOrdersCommand(repositoryFactory.CreateOrdersRepository()),
            CommandType.StartOrderPayment => new StartOrderPaymentCommand(repositoryFactory.CreateOrdersRepository()),
            CommandType.SelectPaymentType => new SelectPaymentTypeCommand(),
            CommandType.TransferMoney => new TransferMoneyCommand(repositoryFactory.CreateOrdersRepository()),
            CommandType.ClearBasket => new ClearBasketCommand(repositoryFactory.CreateBasketRepository()),
            _ => throw new NotSupportedException()
        };
    }

    public ICommandWithCommandsList GetInitialCommand()
    {
        return new InitialCommand(Title);
    }
}
