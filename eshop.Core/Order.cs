using System.Text;

namespace eshop.Core;

/// <summary>
/// Заказ
/// </summary>
public class Order
{
    /// <summary>
    /// Линии заказа
    /// </summary>
    public IReadOnlyCollection<ItemsListLine> Lines { get; }
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public int Id { get; private set; }
    
    /// <summary>
    /// Статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    public decimal Sum => Lines.Sum(l => l.LineSum);
    
    /// <summary>
    /// Признак наличия изменений
    /// </summary>
    public bool HasChanges { get; private set; }

    /// <inheritdoc cref="Order"/>
    public Order(IEnumerable<ItemsListLine> lines)
    {
        Status = OrderStatus.New;
        Lines = lines.ToArray();
    }
    
    /// <inheritdoc cref="Order"/>
    public Order(int id, OrderStatus status, IEnumerable<ItemsListLine> lines)
    {
        Id = id;
        Status = status;
        Lines = lines.ToArray();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Lines.Count == 0)
            return $"Заказ {Id} пуст";
        
        var result = new StringBuilder();
        result.AppendLine($"Заказ {Id}:");
        result.AppendLine($"Статус заказа: {Status}");
        
        for (var i = 0; i < Lines.Count; i++)
        {
            var line = Lines.ElementAt(i);
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

        HasChanges = true;
        Status = OrderStatus.Paid;
        return true;
    }
}