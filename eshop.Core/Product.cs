namespace eshop.Core;

/// <summary>
/// Товар
/// </summary>
public class Product
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id;

    /// <summary>
    /// Наименование
    /// </summary>
    public string Name;

    /// <summary>
    /// Цена
    /// </summary>
    public decimal Price;
    
    /// <summary>
    /// Остатки
    /// </summary>
    public int Remains;

    /// <inheritdoc cref="Product"/>
    public Product(int id, string name, decimal price, int remains)
    {
        Id = id;
        Name = name;
        Price = price;
        Remains = remains;
    }
}