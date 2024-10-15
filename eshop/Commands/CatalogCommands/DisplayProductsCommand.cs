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
    private readonly Product[] _products;

    /// <inheritdoc cref="DisplayProductsCommand"/>
    public DisplayProductsCommand(Product[] products)
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
            count = _products.Length;
        }

        var message = new StringBuilder("Товары:").AppendLine();
        for (var i = 0; i < Math.Min(_products.Length, count); i++)
        {
            message.AppendLine(_products[i].GetDisplayText());
        }

        Result = message.ToString();
    }
}