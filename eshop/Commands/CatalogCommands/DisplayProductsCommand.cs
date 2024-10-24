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
    private readonly IReadOnlyRepositoryAsync<Product> _products;

    /// <inheritdoc cref="DisplayProductsCommand"/>
    public DisplayProductsCommand(IReadOnlyRepositoryAsync<Product> products)
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
        Task.Run(async () =>
        {
            if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
            {
                count = await _products.GetCountAsync();
            }

            var allItems = _products.GetAllAsync().Result;

            var message = new StringBuilder("Товары:").AppendLine();
            for (var i = 0; i < Math.Min(await _products.GetCountAsync(), count); i++)
            {
                message.AppendLine(allItems.ElementAt(i).GetDisplayText());
            }

            Result = message.ToString();
        }).Wait();
    }
}