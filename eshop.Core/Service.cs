namespace eshop.Core;

/// <summary>
/// Услуга
/// </summary>
public class Service
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

    /// <inheritdoc cref="Service"/>
    public Service(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
    
    /// <summary>
    /// Получить строку для отображения услуги
    /// </summary>
    public string GetDisplayText()
    {
        return $"{Id}. {Name}. Цена: {Price:F2}";
    }
}