using eshop.Core;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Payment
{
    public class PayOrderByCashHandler : PayOrderBaseHandler
    {
        public PayOrderByCashHandler(IRepository<Core.Order> ordersRepository, ILogger<PayOrderByCashHandler> logger)
            : base(ordersRepository, logger)
        {
        }

        public async Task<Result<PaymentResult>> PayHandler(int orderId, decimal amount, CancellationToken cancellationToken)
        {
            return await PayAsync(orderId, amount, cancellationToken);
        }

        protected override Task<Result<PaymentResult>> PaymentProcessingAsync(Core.Order order, decimal amount)
        {
            var sum = order.Sum;

            if (amount < sum)
                return Task.FromResult<Result<PaymentResult>>(Result.Fail("Недостаточно средств"));

            if (amount == sum)
                return Task.FromResult(Result.Ok(new PaymentResult(true, "Заказ оплачен")));

            return Task.FromResult(Result.Ok(new PaymentResult(true, $"Заказ оплачен, сдача {amount - sum:F2}")));
        }
    }
}