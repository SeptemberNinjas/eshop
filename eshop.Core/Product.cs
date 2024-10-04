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
    public int Stock;

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