namespace eshop.Core;

/// <summary>
/// Линия списка торговой единицы
/// </summary>
public class ItemsListLine
{
    private readonly SaleItem _lineItem;

    /// <summary>
    /// Идентификатор элемента
    /// </summary>
    public int ItemId => _lineItem.Id;

    /// <summary>
    /// Тип элемента
    /// </summary>
    public ItemTypes ItemType => _lineItem.ItemType;

    /// <summary>
    /// Текст, отображаемый в списке элементов
    /// </summary>
    public string Text => $"{ItemType}: {_lineItem?.Name} | Цена: {_lineItem?.Price:F2} | Кол-во: {Count}";

    /// <summary>
    /// Количество элементов в линии
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Суммарная стоимость по линии
    /// </summary>
    public decimal LineSum => (_lineItem?.Price ?? 0) * Count;

    /// <inheritdoc cref="ItemsListLine"/>
    public ItemsListLine(Product product, int requestedCount)
    {
        _lineItem = product;
        Count = requestedCount;
    }
    
    /// <inheritdoc cref="ItemsListLine"/>
    public ItemsListLine(Service service)
    {
        _lineItem = service;
        Count = 1;
    }
}