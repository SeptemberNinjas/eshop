using System.Text;

namespace eshop.Core;

/// <summary>
/// Заказ
/// </summary>
public class Order
{
    private readonly List<ItemsListLine> _lines;
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public int Id { get; private set; }
    
    /// <summary>
    /// Статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    public decimal Sum => _lines.Sum(l => l.LineSum);
    
    /// <summary>
    /// Признак наличия изменений
    /// </summary>
    public bool HasChanges { get; private set; }

    /// <inheritdoc cref="Order"/>
    public Order(List<ItemsListLine> lines)
    {
        Status = OrderStatus.New;
        _lines = lines;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (_lines.Count == 0)
            return $"Заказ {Id} пуст";
        
        var result = new StringBuilder();
        result.AppendLine($"Заказ {Id}:");
        result.AppendLine($"Статус заказа: {Status}");
        
        for (var i = 0; i < _lines.Count; i++)
        {
            var line = _lines[i];
            result.AppendLine($"{i+1}. {line.Text}");
        }

        result.AppendLine($"Итого: {Sum:F2}");

        return result.ToString();
    }

    /// <summary>
    /// Присвоение идентификатора новому заказу
    /// </summary>
    public void SetNewOrderId(int id)
    {
        if (Status is not OrderStatus.New)
            throw new ApplicationException("Идентификатор можно присвоить только новому заказу");
        
        if (Id != default)
            return;

        Id = id;
    }
    
    /// <summary>
    /// Пометить заказ как оплаченный
    /// </summary>
    public bool SetPaidStatus()
    {
        if (Status is not OrderStatus.New)
            return false;

        Status = OrderStatus.Paid;
        return true;
    }
}