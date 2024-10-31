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
    private readonly IReadOnlyRepositoryAsync<SaleItem> _saleItems;

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
    public DisplayServicesCommand(IReadOnlyRepositoryAsync<SaleItem> saleItems)
    {
        _saleItems = saleItems;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        var allItems = (await _saleItems.GetAllAsync(cancellationToken))
            .Where(i => i.ItemType is ItemTypes.Service)
            .ToArray();

        if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = allItems.Length;
        }

        var message = new StringBuilder("Услуги:").AppendLine();
        for (var i = 0; i < Math.Min(allItems.Length, count); i++)
        {
            message.AppendLine(allItems[i].GetDisplayText());
        }

        Result = message.ToString();
    }

    public void Execute(string[]? args)
    {
        ExecuteAsync(args, CancellationToken.None).Wait();
    }
}