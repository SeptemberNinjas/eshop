using eshop.Commands;
using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;
using eshop.Core.DAL.InMemory;
using eshop.Core.DAL.Json;

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

    /// <summary>
    /// Фабрика, для создания репозитория с продуктами
    /// </summary>
    private readonly RepositoyFactory<Product> _productRepositoyFactory;

    /// <summary>
    /// Список товаров
    /// </summary>
    private readonly IRepository<Product> _products;

    /// <summary>
    /// Фабрика, для создания репозитория с услугами
    /// </summary>
    private readonly RepositoyFactory<Service> _serviceRepositoryFactory;

    /// <summary>
    /// Список услуг
    /// </summary>
    private readonly IRepository<Service>_services;

    private readonly Basket _basket = new();
    private readonly List<Order> _orders = [new Order([new ItemsListLine(new Product(1, "Лопата", 9.99m, 3), 3)])];
    
    public ApplicationContext()
    {
        _productRepositoyFactory = new ProductInJsonRepositoryFactory();
        _serviceRepositoryFactory = new ServiceInMemoryRepositoryFactory();

        _products = _productRepositoyFactory.Create();
        _services = _serviceRepositoryFactory.Create();
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
