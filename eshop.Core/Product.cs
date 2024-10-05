namespace eshop.Core;

/// <summary>
/// Товар
/// </summary>
public class Product
{
    private int _stock;

    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Наименование
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Цена
    /// </summary>
    public decimal Price { get; }

    /// <summary>
    /// Остатки
    /// </summary>
    public int Stock
    {
        get => _stock;
        set => _stock = value < 0 ? 0 : value;
    }

    /// <inheritdoc cref="Product"/>
    public Product(int id, string name, decimal price, int stock)
    {
        Id = id;
        Name = name;
        Price = price;
        Stock = stock;
    }

    /// <summary>
    /// Получить строку для отображения товара
    /// </summary>
    public string GetDisplayText()
    {
        return $"{Id}. {Name}. Цена: {Price:F2}. Остаток: {Stock}";
    }
}