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

        public async Task<Result> PayHandler(int orderId, decimal amount, CancellationToken cancellationToken)
        {
            return await PayAsync(orderId, amount, cancellationToken);
        }

        protected override Result<string> PaymentProcessing(Core.Order order, decimal amount)
        {
            var sum = order.Sum;

            if (amount < sum)
                return Result.Fail("Недостаточно средств");

            if (amount == sum)
                return Result.Ok("Заказ оплачен");

            return Result.Ok($"Заказ оплачен, сдача {amount - sum:F2}");
        }
    }
}