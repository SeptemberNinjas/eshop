using eshop.Core;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда создания заказа
/// </summary>
public class CreateOrderCommand : IEshopCommand
{
    private readonly IRepository<Basket> _basket;
    private readonly IRepository<Order> _orders;

    /// <inheritdoc cref="CreateOrderCommand"/>
    public CreateOrderCommand(IRepository<Basket> basket, IRepository<Order> orders)
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
        var currentBasket = _basket.GetById(default);
        var order = currentBasket?.CreateOrderFromBasket();
        if (order is null)
        {
            Result = "Ошибка при создании заказа. Корзина пуста";
            return;
        }
                
        var id = _orders.Insert(order);
        _basket.Update(currentBasket!);

        Result = $"Создан заказ {id}";
    }
}