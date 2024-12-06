using eshop.DAL;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Payment
{
    public class PayOrderByCashlessHandler : PayOrderBaseHandler
    {
        public PayOrderByCashlessHandler(RepositoryFactory repositoryFactory, ILogger<PayOrderByCashlessHandler> logger) 
            : base(repositoryFactory, logger)
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

            if (amount != sum)
                return Result.Fail($"Внесите ровно {sum}, при безналичном платеже переплата не допускается.");

            return Result.Ok("Заказ оплачен");
        }
    }
}
