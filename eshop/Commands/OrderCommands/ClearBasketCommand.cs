using eshop.Application.Order;

namespace eshop.Commands.OrderCommands;

public class ClearBasketCommand : IEshopCommand
{
    private readonly ClearBasketHandler _handler;

    /// <inheritdoc cref="CreateOrderCommand"/>
    public ClearBasketCommand(ClearBasketHandler handler)
    {
        _handler = handler;
    }
   
    public const string Info = "Очистить корзину";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }

    /// <inheritdoc />
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        var result = await _handler.ClearBasketAsync(args?[0] ?? string.Empty, cancellationToken);
        Result = result.ToString();
    }
}
