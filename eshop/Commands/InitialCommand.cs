using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.SystemCommands;

namespace eshop.Commands;

/// <summary>
/// Команда отображения Основного меню приложения
/// </summary>
public class InitialCommand : ICommandWithCommandsList
{
    private readonly string _title;

    public InitialCommand(string title)
    {
        _title = title;
    }

    public string? Result { get; private set; }
    public bool ExecutionSuccess => true;

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.DisplayBasket, DisplayBasketCommand.Info },
        { CommandType.DisplayOrders, DisplayOrdersCommand.Info },
        { CommandType.DisplaySaleItems, DisplaySaleItemsCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        Result = $"{_title}{Environment.NewLine}{Environment.NewLine}Используйте команды для навигации по страницам";
    }
}