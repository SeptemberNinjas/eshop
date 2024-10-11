using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда добавления элемента в корзину
/// </summary>
public class AddBasketLineCommand
{
    private readonly Basket _basket;
    private readonly Product[] _products;
    private readonly Service[] _services;
    
    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "AddBasketLine";
    
    /// <inheritdoc cref="AddBasketLineCommand"/>
    public AddBasketLineCommand(Basket basket, Product[] products, Service[] services)
    {
        _basket = basket;
        _products = products;
        _services = services;
    }
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Добавить товар в корзину";
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public string Execute(string[]? args)
    {
        if (args is null 
            || args.Length < 2 
            || !int.TryParse(args[0], out var id) 
            || !Enum.TryParse<ItemTypes>(args[1], out var type))
            return "Для добавления в корзину необходимо указать идентификатор, тип (товар или услуга) и количество (для товара)\n" +
                   $"Пример: {Name} 1 {ItemTypes.Product.ToString()} 3";

        var count = 0;
        if (type is ItemTypes.Product && (args.Length < 3 || !int.TryParse(args[2], out count)))
            return "Для добавления в корзину необходимо указать идентификатор, тип (товар или услуга) и количество (для товара)\n" +
                   $"Пример: {Name} 1 {ItemTypes.Product.ToString()} 3";

        switch (type)
        {
            case ItemTypes.Product:
                if (!TryGetItem(id, _products, out var product))
                    return $"Не найден товар с идентификатором {id}";
                return _basket.AddLine(product, count);
            case ItemTypes.Service:
                if (!TryGetItem(id, _services, out var service))
                    return $"Не найдена услуга с идентификатором {id}";
                return _basket.AddLine(service);
            default:
                return string.Empty;
        }
    }

    private static bool TryGetItem<T>(int id, IEnumerable<T> items, out T item)
        where T: SaleItem
    {
        foreach (var saleItem in items)
        {
            if (saleItem.Id != id)
                continue;
            item = saleItem;
            return true;
        }

        item = null!;
        return false;
    }
}