using System.Text;
using eshop.Core.Payment;

namespace eshop.Commands.PaymentCommands;

public class SelectPaymentTypeCommand : IEshopCommand, ICommandWithContext
{
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

        if (args is null || args.Length < 1 || !int.TryParse(args[0], out var type))
        {
            Result = "Не верный тип оплаты";
            return;
        }

        payment.PaymentType = (PaymentType)type;
        ExecutionSuccess = true;
        Result = "Выбран тип оплаты";
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