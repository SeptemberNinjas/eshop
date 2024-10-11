namespace eshop.Core;

/// <summary>
/// Линия списка товаров
/// </summary>
public class ItemsListLine
{
    private readonly Product? _product;
    private readonly Service? _service;
    private int _count;

    /// <summary>
    /// Идентификатор элемента
    /// </summary>
    public int ItemId => _product?.Id ?? _service!.Id;

    /// <summary>
    /// Тип элемента
    /// </summary>
    public ItemTypes ItemType => _product is not null ? ItemTypes.Product : ItemTypes.Service;

    /// <summary>
    /// Текст, отображаемый в списке элементов
    /// </summary>
    public string Text => $"{ItemType}: {_product?.Name ?? _service!.Name} | Цена: {_product?.Price ?? _service!.Price:F2} | Кол-во: {Count}";

    /// <summary>
    /// Количество элементов в линии
    /// </summary>
    public int Count
    {
        get => _count;
        set
        {
            if (_service is not null || value < 1)
                return;

            _count = value;
        }
    }

    /// <summary>
    /// Суммарная стоимость по линии
    /// </summary>
    public decimal LineSum => (_product?.Price ?? _service!.Price) * Count;

    /// <inheritdoc cref="ItemsListLine"/>
    public ItemsListLine(Product product, int requestedCount)
    {
        _product = product;
        Count = requestedCount;
    }
    
    /// <inheritdoc cref="ItemsListLine"/>
    public ItemsListLine(Service service)
    {
        _service = service;
        _count = 1;
    }
}