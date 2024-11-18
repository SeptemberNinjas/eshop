using eshop.DAL;
using FluentResults;

namespace eshop.Application.Payment
{
    public abstract class PayOrderBaseHandler
    {
        private readonly RepositoryFactory _repositoryFactory;

        public PayOrderBaseHandler(RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        protected async Task<Result> PayAsync(int orderId, decimal amount, CancellationToken cancellationToken)
        {
            try
            {
                var ordersRepository = _repositoryFactory.CreateOrdersRepository();

                var order = await ordersRepository.GetByIdAsync(orderId, cancellationToken);

                if (order == null)
                    return Result.Fail($"Заказ с идентификатором {orderId} не найден");

                if (order.Status is not Core.OrderStatus.New)
                    return Result.Fail($"Заказ с идентификатором {orderId} нельзя оплатить");

                var result = PaymentProcessing(order, amount);

                if (result.IsFailed)
                    return Result.Fail("Не удалось выполнить оплату")
                        .WithErrors(result.Errors);

                if (order.SetPaidStatus())
                {
                    await ordersRepository.UpdateAsync(order, cancellationToken);
                }

                return Result.Ok()
                    .WithSuccess(result.Value);
            }
            catch (Exception ex)
            {
                return Result.Fail("Не удалось оплатить заказ")
                    .WithError(ex.Message)
                    .WithError(ex.StackTrace);
            }
        }

        protected abstract Result<string> PaymentProcessing(Core.Order order, decimal amount);
    }
}
