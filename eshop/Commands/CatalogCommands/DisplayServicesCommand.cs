using System.Text;
using eshop.Commands.OrderCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;

namespace eshop.Commands.CatalogCommands;

/// <summary>
/// Команда отображения списка услуг
/// </summary>
public class DisplayServicesCommand : ICommandWithCommandsList
{
    private readonly IRepository<Service> _services;

    public string? Result { get; private set; }
    public bool ExecutionSuccess => true;

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.AddServiceToBasket, AddBasketLineCommand.Info },
        { CommandType.DisplayServices, Info },
        { CommandType.GoToRoot, GoToRootPageCommand.Info },
        { CommandType.Back, BackCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };
    
    public const string Info = "Вывести список услуг";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    /// <inheritdoc cref="DisplayServicesCommand"/>
    public DisplayServicesCommand(IRepository<Service> services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = _services.GetCount();
        }

        var allItems = _services.GetAll();

        var message = new StringBuilder("Услуги:").AppendLine();
        for (var i = 0; i < Math.Min(_services.GetCount(), count); i++)
        {
            message.AppendLine(allItems.ElementAt(i).GetDisplayText());
        }

        Result = message.ToString();
    }
}