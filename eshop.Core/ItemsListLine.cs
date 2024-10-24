namespace eshop.Core;

/// <summary>
/// Линия списка торговой единицы
/// </summary>
public class ItemsListLine
{
    /// <summary>
    /// Продажная единица
    /// </summary>
    public SaleItem SaleItem { get; }

    /// <summary>
    /// Идентификатор элемента
    /// </summary>
    public int ItemId => SaleItem.Id;

    /// <summary>
    /// Тип элемента
    /// </summary>
    public ItemTypes ItemType => SaleItem.ItemType;

    /// <summary>
    /// Текст, отображаемый в списке элементов
    /// </summary>
    public string Text => $"{ItemType.GetDisplayText()}: {SaleItem?.Name} | Цена: {SaleItem?.Price:F2} | Кол-во: {Count}";

    /// <summary>
    /// Количество элементов в линии
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Суммарная стоимость по линии
    /// </summary>
    public decimal LineSum => (SaleItem?.Price ?? 0) * Count;

    /// <inheritdoc cref="ItemsListLine"/>
    public ItemsListLine(Product product, int requestedCount)
    {
        SaleItem = product;
        Count = requestedCount;
    }
    
    /// <inheritdoc cref="ItemsListLine"/>
    public ItemsListLine(Service service)
    {
        SaleItem = service;
        Count = 1;
    }
    
    /// <inheritdoc cref="ItemsListLine"/>
    public ItemsListLine(SaleItem saleItem, int count)
    {
        SaleItem = saleItem;
        Count = count;
    }
}