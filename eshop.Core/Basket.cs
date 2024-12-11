using System.Text;

namespace eshop.Core;

/// <summary>
/// Корзина
/// </summary>
public class Basket
{
    private readonly List<ItemsListLine> _lines;

    public int Id { get; }

    /// <summary>
    /// Линии корзины
    /// </summary>
    public IReadOnlyCollection<ItemsListLine> Lines => _lines;
    
    public string Customer { get; }
    
    public DateTime LastUpdate { get; }

    public Basket(string customer)
    {
        Customer = customer;
        _lines = [];
    }

    public Basket(int id, IEnumerable<ItemsListLine> lines, string customer, DateTime lastUpdate)
    {
        Id = id;
        Customer = customer;
        LastUpdate = lastUpdate;
        _lines = lines.ToList();
    }

    public void AddLine(Service service)
    {
        _lines.Add(new ItemsListLine(service));
    }

    public void AddLine(Product product, int count)
    {
        _lines.Add(new ItemsListLine(product, count));
    }

    /// <summary>
    /// Преобразовать корзину в заказ
    /// </summary>
    public Order? CreateOrderFromBasket()
    {
        if (Lines.Count == 0)
            return null;

        // Создаём копию списка, иначе список линий очистится и в заказе.
        var order = new Order(_lines.ToArray());
        _lines.Clear();

        return order;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Lines.Count == 0)
            return "Корзина пуста";

        var result = new StringBuilder();
        result.AppendLine("Корзина:");

        var total = 0m;
        for (var i = 0; i < Lines.Count; i++)
        {
            var line = _lines[i];
            result.AppendLine($"{i + 1}. {line.Text}");
            total += line.LineSum;
        }

        result.AppendLine($"Итого: {total:F2}");

        return result.ToString();
    }

    public void Clear()
    {
        _lines.Clear();
    }
}