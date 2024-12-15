using System.Text;
using eshop.Application.Order;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда отображения заказов 
/// </summary>
public class DisplayOrdersCommand : ICommandWithCommandsList
{
    private readonly GetOrdersHandler _handler;
    
    /// <inheritdoc cref="DisplayOrdersCommand"/>
    public DisplayOrdersCommand(GetOrdersHandler handler)
    {
        _handler = handler;
    }

    public string? Result { get; private set; }
    public bool ExecutionSuccess { get; private set; }

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
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        var ordersListResult = await _handler.GetOrdersAsync(string.Empty, cancellationToken);
        ExecutionSuccess = ordersListResult.IsSuccess;
        if (ordersListResult.IsFailed)
        {
            Result = ordersListResult.ToString();
            return;
        }
            
        if (!ordersListResult.Value.Any())
        {
            Result = "Список заказов пуст";
            return;
        }
        
        var result = new StringBuilder();
        foreach (var order in ordersListResult.Value)
        {
            result.AppendLine(order.ToString());
        }
        
        Result = result.ToString();
    }
}