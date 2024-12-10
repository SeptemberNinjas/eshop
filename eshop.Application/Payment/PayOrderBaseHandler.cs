using eshop.DAL;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Payment
{
    public abstract class PayOrderBaseHandler
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly ILogger<PayOrderBaseHandler> _logger;

        public PayOrderBaseHandler(RepositoryFactory repositoryFactory, ILogger<PayOrderBaseHandler> logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
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
                _logger.LogError(ex, "Ошибка при оплате закза. {message}", ex.Message);
                
                return Result.Fail("Не удалось оплатить заказ");
            }
        }

        protected abstract Result<string> PaymentProcessing(Core.Order order, decimal amount);
    }
}
