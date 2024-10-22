using System.Text;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда отображения заказов 
/// </summary>
public class DisplayOrdersCommand : ICommandWithCommandsList
{
    private readonly IRepository<Order> _orders;
    
    /// <inheritdoc cref="DisplayOrdersCommand"/>
    public DisplayOrdersCommand(IRepository<Order> orders)
    {
        _orders = orders;
    }

    public string? Result { get; private set; }
    public bool ExecutionSuccess => true;

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.StartOrderPayment, StartOrderPaymentCommand.Info },
        { CommandType.Back, BackCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };

    public const string Info = "Отобразить заказы";

    /// <inheritdoc />
    public override string ToString() => Info;

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        var ordersList = _orders.GetAll();
        if (ordersList.Count == 0)
        {
            Result = "Список заказов пуст";
            return;
        }
        
        var result = new StringBuilder();
        foreach (var order in ordersList)
        {
            result.AppendLine(order.ToString());
        }
        
        Result = result.ToString();
    }
}