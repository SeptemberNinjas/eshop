using eshop.Core;
using eshop.Core.Payment;

namespace eshop.Commands.PaymentCommands;

public class TransferMoneyCommand : IEshopCommand, ICommandWithContext
{
    private readonly List<Order> _orders;

    public TransferMoneyCommand(List<Order> orders)
    {
        _orders = orders;
    }

    public object? Context { get; set; }
    
    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        if (Context is not Payment payment)
        {
            Result = "Не удалось найти оплату";
            return;
        }
        
        if (payment.IsComplete)
        {
            Result = "Оплата уже завершена";
            return;
        }
        
        if (payment.PaymentType is not PaymentType.Cash or PaymentType.Сashless)
        {
            Result = "Не задан способ оплаты";
            return;
        }

        if (args is null || args.Length < 1 || !decimal.TryParse(args[0], out var amount))
        {
            Result = "Указана некорректная сумма";
            return;
        }
        
        var order = _orders.FirstOrDefault(o => o.Id == payment.OrderId);
        payment.CompletePayment(order, amount, out var message);
        ExecutionSuccess = payment.IsComplete;
        Result = message;
    }

    public static string Info => "Внесите денежную сумму";

    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }
    public bool ExecutionSuccess { get; private set; }
}