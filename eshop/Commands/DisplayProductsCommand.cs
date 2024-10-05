using System.Text;
using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда отображения списка товаров
/// </summary>
public class DisplayProductsCommand
{
    private readonly Product[] _products;
    
    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "DisplayProducts";

    /// <inheritdoc cref="DisplayProductsCommand"/>
    public DisplayProductsCommand(Product[] products)
    {
        _products = products;
    }
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Вывести список товаров";
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public string Execute(string[]? args)
    {
        if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = _products.Length;
        }

        var message = new StringBuilder();
        for (var i = 0; i < Math.Min(_products.Length, count); i++)
        {
            message.AppendLine(_products[i].GetDisplayText());
        }

        return message.ToString();
    }
}