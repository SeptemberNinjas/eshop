using eshop.Application.Order;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда добавления элемента в корзину
/// </summary>
public class AddBasketLineCommand : IEshopCommand
{
    private readonly AddBasketLineHandler _handler;
    private const string ArgsErrorMessage = "Для добавления в корзину необходимо указать идентификатор и количество (для товара)";
    
    /// <inheritdoc cref="AddBasketLineCommand"/>
    public AddBasketLineCommand(AddBasketLineHandler handler)
    {
        _handler = handler;
    }

    public const string Info = "Добавить позицию в корзину";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }

    /// <inheritdoc />
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        if (args is null 
            || args.Length < 1 
            || !int.TryParse(args[0], out var id))
        {
            Result = ArgsErrorMessage;
            return;
        }
        
        var count = args.Length < 2 || !int.TryParse(args[1], out var countFromArgs) ? 0 : countFromArgs;
        var result = await _handler.AddLineAsync(id, count, cancellationToken);
        Result = result.ToString();
    }
}