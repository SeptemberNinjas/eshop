using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using eshop.Core;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Payment
{
    public class PayOrderByCashlessHandler : PayOrderBaseHandler
    {
        private readonly ILogger<PayOrderByCashlessHandler> _logger;
        private readonly HttpClient _client;

        public PayOrderByCashlessHandler(IRepository<Core.Order> orderRepository, ILogger<PayOrderByCashlessHandler> logger, HttpClient client) 
            : base(orderRepository, logger)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<Result<PaymentResult>> PayHandler(int orderId, decimal amount, CancellationToken cancellationToken)
        {
            return await PayAsync(orderId, amount, cancellationToken);
        }

        protected override async Task<Result<PaymentResult>> PaymentProcessingAsync(Core.Order order, decimal amount)
        {
            var sum = order.Sum;

            if (amount < sum)
                return Result.Fail("Недостаточно средств");

            if (amount != sum)
                return Result.Fail($"Внесите ровно {sum}, при безналичном платеже переплата не допускается.");

            var requestBody = new PaymentRequest(order.Id, amount);
            var request = new HttpRequestMessage(HttpMethod.Post, "Payment");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            var response = await _client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                const string message = "Ошибка при выполнении оплаты";
                using var _ = _logger.BeginScope(new Dictionary<string, object> {{"response", response}});
                _logger.LogError(message);
                return Result.Fail(message);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PaymentResult>(content);
            if (result is null)
                return Result.Fail("Не удалось прочитать ответ");
            
            return Result.Ok(result);
        }
    }
}
