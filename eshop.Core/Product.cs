namespace eshop.Core;

/// <summary>
/// Товар
/// </summary>
public class Product : SaleItem
{
    private int _stock;

    /// <inheritdoc/>
    public override ItemTypes ItemType => ItemTypes.Product;

    /// <summary>
    /// Остатки
    /// </summary>
    public int Stock
    {
        get => _stock;
        set => _stock = value < 0 ? 0 : value;
    }

    /// <inheritdoc cref="Product"/>
    public Product(int id, string name, decimal price, int stock) : base(id, name, price)
    {
        Stock = stock;
    }

    /// <inheritdoc/>
    public override string GetDisplayText()
    {
        return $"{Id}. {Name}. Цена: {Price:F2}. Остаток: {Stock}";
    }
}