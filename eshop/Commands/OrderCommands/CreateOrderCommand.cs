using eshop.Core;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда создания заказа
/// </summary>
public class CreateOrderCommand : IEshopCommand
{
    private readonly Basket _basket;
    private readonly List<Order> _orders;

    /// <inheritdoc cref="CreateOrderCommand"/>
    public CreateOrderCommand(Basket basket, List<Order> orders)
    {
        _basket = basket;
        _orders = orders;
    }
   
    public const string Info = "Создать заказ из текущей корзины";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        var order = _basket.CreateOrderFromBasket();
        if (order is null)
        {
            Result = "Ошибка при создании заказа. Корзина пуста";
            return;
        }
                
        _orders.Add(order);

        Result = $"Создан заказ {order.Id}";
    }
}