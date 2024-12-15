using System.Text.Json.Serialization;

namespace eshop.Application.Payment;

[Serializable]
public record PaymentResult(
    [property: JsonPropertyName("isSuccess"), JsonRequired]
    bool IsSuccess, 
    [property: JsonPropertyName("message"), JsonRequired]
    string Message);