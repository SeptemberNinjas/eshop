using System.Text;

namespace eshop.Core;

/// <summary>
/// Корзина
/// </summary>
public class Basket
{
    private readonly List<ItemsListLine> _lines = new ();

    /// <summary>
    /// Добавить товар в корзину
    /// </summary>
    public string AddLine(Product product, int requestedCount)
    {
        if (requestedCount < 1)
            return "Запрашиваемое количество товара должно быть больше 0";
        
        if (product.Stock < requestedCount)
            return $"Нельзя добавить товар в корзину, недостаточно остатков. Имеется {product.Stock}, Требуется {requestedCount}";

        product.Stock -= requestedCount;
        
        if (IsLineExists(product, out var line))
            line.Count += requestedCount;
        else
            _lines.Add(new ItemsListLine(product, requestedCount));

        return $"В корзину добавлено {requestedCount} единиц товара \'{product.Name}\'";
    }
    
    /// <summary>
    /// Добавить услугу в корзину
    /// </summary>
    public string AddLine(Service service)
    {
        if (IsLineExists(service, out _))
            return $"Ошибка при добавлении услуги. Услуга \'{service.Name}\' уже добавлена в корзину";
        
        _lines.Add(new ItemsListLine(service));
        return $"В корзину добавлена услуга \'{service.Name}\'";
    }
    
    /// <summary>
    /// Преобразовать корзину в заказ
    /// </summary>
    public Order? CreateOrderFromBasket()
    {
        if (_lines.Count == 0)
            return null;

        // Создаём копию списка, иначе список линий очистится и в заказе.
        var order = new Order(_lines.ToList());
        _lines.Clear();

        return order;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (_lines.Count == 0)
            return "Корзина пуста";
        
        var result = new StringBuilder();
        result.AppendLine("Корзина:");
        
        var total = 0m;
        for (var i = 0; i < _lines.Count; i++)
        {
            var line = _lines[i];
            result.AppendLine($"{i+1}. {line.Text}");
            total += line.LineSum;
        }

        result.AppendLine($"Итого: {total:F2}");

        return result.ToString();
    }

    private bool IsLineExists(Product product, out ItemsListLine line)
    {
        foreach (var ln in _lines)
        {
            if (ln.ItemType != ItemTypes.Product || ln.ItemId != product.Id) 
                continue;
            line = ln;
            return true;
        }

        line = null!;
        return false;
    }
    
    private bool IsLineExists(Service service, out ItemsListLine line)
    {
        foreach (var ln in _lines)
        {
            if (ln.ItemType != ItemTypes.Service || ln.ItemId != service.Id) 
                continue;
            line = ln;
            return true;
        }

        line = null!;
        return false;
    }
}