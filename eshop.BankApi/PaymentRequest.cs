namespace eshop.BankApi;

public record PaymentRequest(int OrderId, decimal Amount);