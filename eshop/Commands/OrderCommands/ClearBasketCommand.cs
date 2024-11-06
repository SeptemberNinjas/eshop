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
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        var currentBasket = await _basket.GetByIdAsync(default);
        if (currentBasket is null)
        {
            Result = "Корзина не найдена";
            return;
        }
        currentBasket.Clear();
        await _basket.UpdateAsync(currentBasket);

        Result = "Корзина очищена";
    }
}
