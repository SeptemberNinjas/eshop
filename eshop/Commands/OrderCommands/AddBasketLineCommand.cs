using eshop.Core;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда добавления элемента в корзину
/// </summary>
public class AddBasketLineCommand : IEshopCommand
{
    private const string ArgsErrorMessage = "Для добавления в корзину необходимо указать идентификатор и количество (для товара)";
    
    private readonly Basket _basket;
    private readonly SaleItem[] _list;
    
    /// <inheritdoc cref="AddBasketLineCommand"/>
    public AddBasketLineCommand(Basket basket, SaleItem[] list)
    {
        _basket = basket;
        _list = list;
    }

    public const string Info = "Добавить позицию в корзину";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        if (args is null 
            || args.Length < 1 
            || !int.TryParse(args[0], out var id))
        {
            Result = ArgsErrorMessage;
            return;
        }

        var count = 0;
        var type = _list.FirstOrDefault()?.ItemType;
        if (type is null || (type is ItemTypes.Product && (args.Length < 2 || !int.TryParse(args[1], out count))))
        {
            Result = ArgsErrorMessage;
            return;
        }

        if (type == ItemTypes.Product)
        {
            if (!TryGetItem(id, _list, out var product))
                Result = $"Не найден товар с идентификатором {id}";
            Result = _basket.AddLine(product as Product, count);
        }
        else if (type == ItemTypes.Service)
        {
            if (!TryGetItem(id, _list, out var service))
                Result = $"Не найдена услуга с идентификатором {id}";
            Result = _basket.AddLine(service as Service);
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