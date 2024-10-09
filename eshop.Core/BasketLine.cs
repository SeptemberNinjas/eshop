namespace eshop.Core;

/// <summary>
/// Линия корзины
/// </summary>
public class BasketLine
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
    /// Текст, отображаемый в списке элементов корзины
    /// </summary>
    public string Text => $"{_product?.Name ?? _service!.Name} | {Count}";

    /// <summary>
    /// Количество элементов в корзине
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

    /// <inheritdoc cref="BasketLine"/>
    public BasketLine(Product product, int requestedCount)
    {
        _product = product;
        Count = requestedCount;
    }
    
    /// <inheritdoc cref="BasketLine"/>
    public BasketLine(Service service)
    {
        _service = service;
        _count = 1;
    }
}