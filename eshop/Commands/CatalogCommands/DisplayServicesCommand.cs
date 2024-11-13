using System.Text;
using eshop.Application.SaleItems;
using eshop.Commands.OrderCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;

namespace eshop.Commands.CatalogCommands;

/// <summary>
/// Команда отображения списка услуг
/// </summary>
public class DisplayServicesCommand : ICommandWithCommandsList
{
    private readonly GetSaleItemHandler _getSaleItemHandler;

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
    public DisplayServicesCommand(GetSaleItemHandler getSaleItemHandler)
    {
        _getSaleItemHandler = getSaleItemHandler;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        _ = int.TryParse(args?.FirstOrDefault(), out var count);

        var items = await _getSaleItemHandler.GetItemsAsync(ItemTypes.Service, count, cancellationToken);

        if (items.IsFailed)
        {
            Result = "Не удалось получить список услуг";
            return;
        }

        var message = new StringBuilder("Услуги:").AppendLine();

        for (var i = 0; i < items.Value.Count(); i++)
        {
            var item = items.Value.ElementAt(i);
            message
                .Append($"{item.Id}. {item.Name}. Цена: {item.Price:F2}")
                .AppendLine();
        }

        Result = message.ToString();
    }
}