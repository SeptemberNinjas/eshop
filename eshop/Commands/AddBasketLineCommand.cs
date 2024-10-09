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
                if (!TryGetProduct(id, out var product))
                    return $"Не найден товар с идентификатором {id}";
                return _basket.AddLine(product, count);
            case ItemTypes.Service:
                if (!TryGetService(id, out var service))
                    return $"Не найдена услуга с идентификатором {id}";
                return _basket.AddLine(service);
            default:
                return string.Empty;
        }
    }

    private bool TryGetService(int id, out Service service)
    {
        foreach (var s in _services)
        {
            if (s.Id != id)
                continue;
            service = s;
            return true;
        }

        service = null!;
        return false;
    }

    private bool TryGetProduct(int id, out Product product)
    {
        foreach (var pr in _products)
        {
            if (pr.Id != id)
                continue;
            product = pr;
            return true;
        }

        product = null!;
        return false;
    }
}