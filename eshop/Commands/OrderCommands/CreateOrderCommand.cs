using eshop.Application.Order;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда создания заказа
/// </summary>
public class CreateOrderCommand : IEshopCommand
{
    private readonly CreateOrderHandler _handler;

    /// <inheritdoc cref="CreateOrderCommand"/>
    public CreateOrderCommand(CreateOrderHandler handler)
    {
        _handler = handler;
    }
   
    public const string Info = "Создать заказ из текущей корзины";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }

    /// <inheritdoc />
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        var result = await _handler.CreateOrderAsync(string.Empty, cancellationToken);
        Result = result.ToString();
    }
}