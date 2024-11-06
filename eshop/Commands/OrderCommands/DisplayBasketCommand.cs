using eshop.Commands.SystemCommands;
using eshop.Core;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда отображения корзины
/// </summary>
public class DisplayBasketCommand : ICommandWithCommandsList
{
    private readonly IReadOnlyRepository<Basket> _basket;

    /// <inheritdoc cref="DisplayBasketCommand"/>
    public DisplayBasketCommand(IReadOnlyRepository<Basket> basket)
    {
        _basket = basket;
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
        var currentBasket = await _basket.GetByIdAsync(default);
        if (currentBasket is null)
        {
            ExecutionSuccess = false;
            Result = "Корзина не найдена";
            return;
        }
        
        ExecutionSuccess = true;
        Result = currentBasket.ToString();
    }
}