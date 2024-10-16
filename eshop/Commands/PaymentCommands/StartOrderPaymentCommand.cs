using eshop.Commands.SystemCommands;
using eshop.Core;
using eshop.Core.Payment;

namespace eshop.Commands.PaymentCommands;

/// <summary>
/// Команда инициализации процесса оплаты
/// </summary>
public class StartOrderPaymentCommand : ICommandWithCommandsList, ICommandWithContext
{
    private readonly List<Order> _orders;
    public object? Context { get; set; }
    
    public StartOrderPaymentCommand(List<Order> orders)
    {
        _orders = orders;
    }

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        if (args is null || args.Length == 0 || !Guid.TryParse(args[0], out var id))
        {
            Result = "Необходимо указать идентификатор заказа для оплаты";
            return;
        }

        var order = _orders.FirstOrDefault(o => o.Id == id);

        if (order is null)
        {
            Result = $"Заказ с идентификатором {id} не найден";
            return;
        }

        if (order.Status is not OrderStatus.New)
        {
            Result = $"Заказ с идентификатором {id} нельзя оплатить";
            return;
        }

        Context = new Payment(order.Id);
        ExecutionSuccess = true;

        Result = $"Оплата заказа {id}:";
    }

    public const string Info = "Оплата заказа (для оплаты укажите идентификатор аргументом в команде оплаты)";

    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }
    public bool ExecutionSuccess { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<CommandType, string> AvailableCommands { get; } = new Dictionary<CommandType, string>
    {
        { CommandType.SelectPaymentType, SelectPaymentTypeCommand.Info },
        { CommandType.TransferMoney, TransferMoneyCommand.Info },
        { CommandType.GoToRoot, GoToRootPageCommand.Info },
        { CommandType.Back, BackCommand.Info },
        { CommandType.Exit, ExitCommand.Info }
    };
}