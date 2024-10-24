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
    private readonly IReadOnlyRepository<Product> _products;

    /// <inheritdoc cref="DisplayProductsCommand"/>
    public DisplayProductsCommand(IReadOnlyRepository<Product> products)
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
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = await _products.GetCountAsync();
        }

        var allItems = await _products.GetAllAsync();

        var message = new StringBuilder("Товары:").AppendLine();
        for (var i = 0; i < Math.Min(await _products.GetCountAsync(), count); i++)
        {
            message.AppendLine(allItems.ElementAt(i).GetDisplayText());
        }

        Result = message.ToString();

    }

    public void Execute(string[]? args)
    {
        ExecuteAsync(args, CancellationToken.None).Wait();
    }
}