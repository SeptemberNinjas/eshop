using eshop.Core;

namespace eshop.Commands.OrderCommands;

public class ClearBasketCommand : IEshopCommand
{
    private readonly IRepository<Basket> _basket;

    /// <inheritdoc cref="CreateOrderCommand"/>
    public ClearBasketCommand(IRepository<Basket> basket)
    {
        _basket = basket;
    }
   
    public const string Info = "Очистить корзину";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        var currentBasket = _basket.GetById(default);
        if (currentBasket is null)
        {
            Result = "Корзина не найдена";
            return;
        }
        currentBasket.Clear();
        _basket.Update(currentBasket);

        Result = "Корзина очищена";
    }
}
