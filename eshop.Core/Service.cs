namespace eshop.Core;

/// <summary>
/// Услуга
/// </summary>
public class Service : SaleItem
{

    /// <summary>
    /// Цена
    /// </summary>
    public decimal Price { get; }

    /// <inheritdoc cref="Service"/>
    public Service(int id, string name, decimal price) : base(id, name)
    {
        Price = price;
    }

    /// <inheritdoc/>
    public override string GetDisplayText()
    {
        return $"{Id}. {Name}. Цена: {Price:F2}";
    }
}