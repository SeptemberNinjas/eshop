using System.Text;
using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда отображения заказов 
/// </summary>
public class DisplayOrdersCommand
{
    private readonly List<Order> _orders;

    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "DisplayOrders";
    
    /// <inheritdoc cref="DisplayOrdersCommand"/>
    public DisplayOrdersCommand(List<Order> orders)
    {
        _orders = orders;
    }
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Отобразить заказы";
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public string Execute()
    {
        if (_orders.Count == 0)
            return "Список заказов пуст";
        
        var result = new StringBuilder();
        foreach (var order in _orders)
        {
            result.AppendLine(order.ToString());
        }

        return result.ToString();
    }
}