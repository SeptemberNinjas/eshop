using eshop.Core;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Payment
{
    public abstract class PayOrderBaseHandler
    {
        private readonly IRepository<Core.Order> _ordersRepository;
        private readonly ILogger<PayOrderBaseHandler> _logger;

        public PayOrderBaseHandler(IRepository<Core.Order> ordersRepository, ILogger<PayOrderBaseHandler> logger)
        {
           
            _ordersRepository = ordersRepository;
            _logger = logger;
        }

        protected async Task<Result> PayAsync(int orderId, decimal amount, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _ordersRepository.GetByIdAsync(orderId, cancellationToken);

                if (order == null)
                    return Result.Fail($"Заказ с идентификатором {orderId} не найден");

                if (order.Status is not OrderStatus.New)
                    return Result.Fail($"Заказ с идентификатором {orderId} нельзя оплатить");

                var result = PaymentProcessing(order, amount);

                if (result.IsFailed)
                    return Result.Fail("Не удалось выполнить оплату")
                        .WithErrors(result.Errors);

                if (order.SetPaidStatus())
                {
                    await _ordersRepository.UpdateAsync(order, cancellationToken);
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
