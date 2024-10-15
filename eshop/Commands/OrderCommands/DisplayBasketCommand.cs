using eshop.Commands.SystemCommands;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда отображения корзины
/// </summary>
public class DisplayBasketCommand : ICommandWithCommandsList
{
    private readonly Core.Basket _basket;

    /// <inheritdoc cref="DisplayBasketCommand"/>
    public DisplayBasketCommand(Core.Basket basket)
    {
        _basket = basket;
    }

    public string? Result { get; private set; }
    public bool ExecutionSuccess => true;

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.CreateOrder, CreateOrderCommand.Info },
        { CommandType.Back, BackCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };

    public const string Info = "Отобразить корзину";

    /// <inheritdoc />
    public override string ToString() => Info;

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        Result = _basket.ToString();
    }
}