using System.Text;
using eshop.Commands.OrderCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;

namespace eshop.Commands.CatalogCommands;

/// <summary>
/// Команда отображения списка товаров
/// </summary>
public class DisplayProductsCommand : ICommandWithCommandsList
{
    private readonly IRepository<Product> _products;

    /// <inheritdoc cref="DisplayProductsCommand"/>
    public DisplayProductsCommand(IRepository<Product> products)
    {
        _products = products;
    }

    public string? Result { get; private set; }
    public bool ExecutionSuccess => true;

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.AddProductToBasket, AddBasketLineCommand.Info },
        { CommandType.DisplayProducts, Info },
        { CommandType.GoToRoot, GoToRootPageCommand.Info },
        { CommandType.Back, BackCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };
    
    public const string Info = "Вывести список товаров";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = _products.GetCount();
        }

        var allItems = _products.GetAll();

        var message = new StringBuilder("Товары:").AppendLine();
        for (var i = 0; i < Math.Min(_products.GetCount(), count); i++)
        {
            message.AppendLine(allItems.ElementAt(i).GetDisplayText());
        }

        Result = message.ToString();
    }
}