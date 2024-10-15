﻿using System.Text;

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
    public Guid Id { get; }
    
    /// <summary>
    /// Статус заказа.
    /// </summary>
    public OrderStatus Status { get; set; }

    public decimal Sum => _lines.Sum(l => l.LineSum);

    /// <inheritdoc cref="Order"/>
    public Order(List<ItemsListLine> lines)
    {
        Status = OrderStatus.New;
        Id = Guid.NewGuid();
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
}