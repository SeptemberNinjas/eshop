using eshop.Application.Order;
using eshop.Commands.SystemCommands;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда отображения корзины
/// </summary>
public class DisplayBasketCommand : ICommandWithCommandsList
{
    private readonly GetBasketHandler _handler;

    /// <inheritdoc cref="DisplayBasketCommand"/>
    public DisplayBasketCommand(GetBasketHandler handler)
    {
        _handler = handler;
    }

    public string? Result { get; private set; }
    public bool ExecutionSuccess { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.ClearBasket, ClearBasketCommand.Info },
        { CommandType.CreateOrder, CreateOrderCommand.Info },
        { CommandType.Back, BackCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };

    public const string Info = "Отобразить корзину";

    /// <inheritdoc />
    public override string ToString() => Info;

    /// <inheritdoc />
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        var result = await _handler.GetBasketAsync(cancellationToken);
        ExecutionSuccess = result.IsSuccess;
        Result = result.Value.ToString();
    }
}