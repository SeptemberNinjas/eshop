namespace eshop.Application.Payment;

public record PaymentRequest(int OrderId, decimal Amount);