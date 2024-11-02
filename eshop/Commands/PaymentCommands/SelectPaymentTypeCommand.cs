using System.Text;
using eshop.Core.Payment;

namespace eshop.Commands.PaymentCommands;

public class SelectPaymentTypeCommand : IEshopCommand, ICommandWithContext
{
    public object? Context { get; set; }
    
    /// <inheritdoc />
    public Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        if (Context is not Payment payment)
        {
            Result = "Не удалось найти оплату";
            return Task.CompletedTask;
        }
        
        if (payment.IsComplete)
        {
            Result = "Оплата уже завершена";
            return Task.CompletedTask;
        }

        if (args is null || args.Length < 1 || !int.TryParse(args[0], out var type))
        {
            Result = "Не верный тип оплаты";
            return Task.CompletedTask;
        }

        payment.PaymentType = (PaymentType)type;
        ExecutionSuccess = true;
        Result = "Выбран тип оплаты";

        return Task.CompletedTask;
    }

    public static string Info => new StringBuilder("Выберите способ оплаты (укажите аргументом в команде):")
        .Append($"({(int)PaymentType.Cash} - Наличные, ")
        .Append($"{(int)PaymentType.Сashless} - Банковская карта)")
        .ToString();

    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }
    public bool ExecutionSuccess { get; private set; }
}