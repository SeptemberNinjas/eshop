using eshop.Commands;
using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;

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
    /// Список товаров
    /// </summary>
    private readonly Product[] _products = new[]
    {
        new Product(1, "Лопата", 9.99m, 3),
        new Product(2, "Трактор", 300, 4)
    };

    /// <summary>
    /// Список услуг
    /// </summary>
    private readonly Service[] _services = new[]
    {
        new Service(3, "Раскопать яму", 5.49m),
        new Service(4, "Вспахать поле", 1000)
    };

    private readonly Basket _basket = new();
    private readonly List<Order> _orders = [new Order([new ItemsListLine(new Product(1, "Лопата", 9.99m, 3), 3)])];
    
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
            CommandType.AddProductToBasket => new AddBasketLineCommand(_basket, _products.Cast<SaleItem>().ToArray()),
            CommandType.AddServiceToBasket => new AddBasketLineCommand(_basket, _services.Cast<SaleItem>().ToArray()),
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
