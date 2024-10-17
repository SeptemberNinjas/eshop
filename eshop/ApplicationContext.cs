using eshop.Commands;
using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;
using eshop.DAL;
using eshop.DAL.JSON;
using eshop.DAL.Memory;

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

    private readonly IRepository<Product> _products;
    private readonly IRepository<Service> _services;

    private readonly Basket _basket = new();
    private readonly List<Order> _orders = [new Order([new ItemsListLine(new Product(1, "Лопата", 9.99m, 3), 3)])];

    private readonly RepositoryFactory _repositoryFactory;

    public ApplicationContext()
    {
        _repositoryFactory = new JsonRepositoryFactory();

        _products = _repositoryFactory.CreateProductRepository();
        _services = _repositoryFactory.CreateServiceRepository();
    }

    public IEshopCommand CreateCommand(CommandType commandType)
    {
        return commandType switch
        {
            CommandType.Exit => new ExitCommand(),
            CommandType.Back => new BackCommand(),
            CommandType.GoToRoot => new GoToRootPageCommand(),
            CommandType.DisplaySaleItems => new DisplaySaleItemsCommand(),
            CommandType.DisplayProducts => new DisplayProductsCommand(_products),
            CommandType.DisplayServices => new DisplayServicesCommand(_services),
            CommandType.DisplayBasket => new DisplayBasketCommand(_basket),
            CommandType.AddProductToBasket => new AddBasketLineCommand(_basket, _products.GetAll().ToArray()),
            CommandType.AddServiceToBasket => new AddBasketLineCommand(_basket, _services.GetAll().ToArray()),
            CommandType.CreateOrder => new CreateOrderCommand(_basket, _orders),
            CommandType.DisplayOrders => new DisplayOrdersCommand(_orders),
            CommandType.StartOrderPayment => new StartOrderPaymentCommand(_orders),
            CommandType.SelectPaymentType => new SelectPaymentTypeCommand(),
            CommandType.TransferMoney => new TransferMoneyCommand(_orders),
            _ => throw new NotSupportedException()
        };
    }

    public ICommandWithCommandsList GetInitialCommand()
    {
        return new InitialCommand(Title);
    }
}
