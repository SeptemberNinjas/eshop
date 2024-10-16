namespace eshop.Core.Payment;

/// <summary>
/// Процесс оплаты
/// </summary>
public class Payment
{
    /// <summary>
    /// Идентификатор заказа в оплате.
    /// </summary>
    public Guid OrderId { get; }

    public PaymentType PaymentType { get; set; }
    
    public bool IsComplete { get; private set; }

    public Payment(Guid orderId)
    {
        OrderId = orderId;
    }

    public void CompletePayment(Order? order, decimal amount, out string message)
    {
        if (!ValidateAmount(order, amount, out message))
            return;
        
        order!.Status = OrderStatus.Paid;
        IsComplete = true;
    }

    private bool ValidateAmount(Order? order, decimal amount, out string message)
    {
        if (order is null)
        {
            message = "Не найден заказ для оплаты";
            return false;
        }

        var sum = order.Sum;
        
        if (amount < sum)
        {
            message = "Недостаточно средств";
            return false;
        }
        
        if (amount == sum)
        {
            message = "Заказ оплачен";
            return true;
        }

        switch (PaymentType)
        {
            case PaymentType.Cash:
                message = $"Заказ оплачен, сдача {amount - sum:F2}";
                return true;
            case PaymentType.Сashless:
                message = $"Внесите ровно {sum}, при безналичном платеже переплата не допускается.";
                return false;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}