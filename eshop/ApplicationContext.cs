using eshop.Commands;
using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;
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
            .AddScoped<RepositoryFactory>((sp) =>
            {
                return new DatabaseRepositoryFactory(configuration["ConnectionString"] ?? "");
            });

        _serviceProvider = services.BuildServiceProvider();
    }

    public IEshopCommand CreateCommand(CommandType commandType)
    {
        using var scope = _serviceProvider.CreateScope();
        var repositoryFactory = scope.ServiceProvider.GetRequiredService<RepositoryFactory>();

        return commandType switch
        {
            CommandType.Exit => new ExitCommand(),
            CommandType.Back => new BackCommand(),
            CommandType.GoToRoot => new GoToRootPageCommand(),
            CommandType.DisplaySaleItems => new DisplaySaleItemsCommand(),
            CommandType.DisplayProducts => new DisplayProductsCommand(repositoryFactory.CreateProductRepository()),
            CommandType.DisplayServices => new DisplayServicesCommand(repositoryFactory.CreateServiceRepository()),
            CommandType.DisplayBasket => new DisplayBasketCommand(repositoryFactory.CreateBasketRepository()),
            CommandType.AddProductToBasket => new AddBasketLineCommand(repositoryFactory.CreateBasketRepository(), (repositoryFactory.CreateProductRepository() as IRepository<SaleItem>)!),
            CommandType.AddServiceToBasket => new AddBasketLineCommand(repositoryFactory.CreateBasketRepository(), (repositoryFactory.CreateServiceRepository() as IReadOnlyRepository<SaleItem>)!),
            CommandType.CreateOrder => new CreateOrderCommand(repositoryFactory),
            CommandType.DisplayOrders => new DisplayOrdersCommand(repositoryFactory.CreateOrdersRepository()),
            CommandType.StartOrderPayment => new StartOrderPaymentCommand(repositoryFactory.CreateOrdersRepository()),
            CommandType.SelectPaymentType => new SelectPaymentTypeCommand(),
            CommandType.TransferMoney => new TransferMoneyCommand(repositoryFactory.CreateOrdersRepository()),
            _ => throw new NotSupportedException()
        };
    }

    public ICommandWithCommandsList GetInitialCommand()
    {
        return new InitialCommand(Title);
    }
}
