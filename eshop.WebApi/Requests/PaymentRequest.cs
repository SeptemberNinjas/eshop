using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace eshop.WebApi.Requests
{
    public class PaymentRequest
    {
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [Range(1, 2, ErrorMessage = "Тип оплаты должен быть: 1 (наличная оплата) или 2 (безналичная оплата)")]
        public byte PaymentType { get; set; }

        [JsonPropertyName("amount")]
        [Range(1, double.MaxValue, ErrorMessage = "Сумма для оплаты должны быть больше нуля")]
        public decimal Amount { get; set; }
    }
}
