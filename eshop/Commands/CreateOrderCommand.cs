using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда создания заказа
/// </summary>
public class CreateOrderCommand
{
    private readonly Basket _basket;
    private readonly List<Order> _orders;
    
    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "CreateOrder";

    /// <inheritdoc cref="CreateOrderCommand"/>
    public CreateOrderCommand(Basket basket, List<Order> orders)
    {
        _basket = basket;
        _orders = orders;
    }
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Создать заказ из текущей корзины";
    }

    /// <summary>
    /// Выполнить команду
    /// </summary>
    public string Execute()
    {
        var order = _basket.CreateOrderFromBasket();
        if (order is null)
            return "Ошибка при создании заказа. Корзина пуста";
                
        _orders.Add(order);

        return $"Создан заказ {order.Id}";
    }
}