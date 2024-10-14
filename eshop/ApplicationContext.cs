using eshop.Commands;
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

    private Basket _basket = new();
    
    private List<Order> _orders = new();

    /// <summary>
    /// Выполнить стартовую команду
    /// </summary>
    public string ExecuteStartupCommand()
    {
        return ExecuteCommandByName(DisplayCommandsCommand.Name);
    }

    /// <summary>
    /// Выполнить команду по имени
    /// </summary>
    public string ExecuteCommandByName(string commandName, string[]? args = null)
    {
        return commandName switch
        {
            DisplayCommandsCommand.Name => DisplayCommandsCommand.Execute(),
            ExitCommand.Name => ExitCommand.Execute(),
            DisplayProductsCommand.Name => new DisplayProductsCommand(_products).Execute(args),
            DisplayServicesCommand.Name => new DisplayServicesCommand(_services).Execute(args),
            AddBasketLineCommand.Name => new AddBasketLineCommand(_basket, _products, _services).Execute(args),
            DisplayBasketCommand.Name => new DisplayBasketCommand(_basket).Execute(),
            CreateOrderCommand.Name => new CreateOrderCommand(_basket, _orders).Execute(),
            DisplayOrdersCommand.Name => new DisplayOrdersCommand(_orders).Execute(),
            var _ => "Ошибка: неизвестная команда"
        };
    }
}