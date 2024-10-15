using eshop.Commands.SystemCommands;

namespace eshop.Commands.CatalogCommands;

/// <summary>
/// Команда отображения меню выбора списков для отображения
/// </summary>
public class DisplaySaleItemsCommand : ICommandWithCommandsList
{
    public string? Result { get; private set; }
    public bool ExecutionSuccess => true;

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.DisplayProducts, DisplayProductsCommand.Info },
        { CommandType.DisplayServices, DisplayServicesCommand.Info },
        { CommandType.Back, BackCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };
    
    public const string Info = "Вывести доступные списки продажных единиц";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        Result = "Выберите необходимый список, опционально можно указать требуемое количество элементов";
    }
}