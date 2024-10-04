using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда отображения списка товаров
/// </summary>
public class DisplayProductsCommand
{
    private readonly Product[] _products;
    
    public const string Name = "DisplayProducts";
    
    public static string GetInfo()
    {
        return "Вывести список товаров";
    }

    /// <inheritdoc cref="DisplayProductsCommand"/>
    public DisplayProductsCommand(Product[] products)
    {
        _products = products;
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public void Execute(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = _products.Length;
        }
 
        for (var i = 0; i < Math.Min(_products.Length, count); i++)
        {
            Console.WriteLine(_products[i].GetDisplayText());
        }
    }
}