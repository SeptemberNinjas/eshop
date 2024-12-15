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

        protected async Task<Result<PaymentResult>> PayAsync(int orderId, decimal amount, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _ordersRepository.GetByIdAsync(orderId, cancellationToken);

                if (order == null)
                    return Result.Fail($"Заказ с идентификатором {orderId} не найден");

                if (order.Status is not OrderStatus.New)
                    return Result.Fail($"Заказ с идентификатором {orderId} нельзя оплатить");

                var result = await PaymentProcessingAsync(order, amount);

                if (result.IsFailed)
                    return Result.Fail("Не удалось выполнить оплату")
                        .WithErrors(result.Errors);

                if (result.Value.IsSuccess && order.SetPaidStatus())
                {
                    await _ordersRepository.UpdateAsync(order, cancellationToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при оплате заказа. {message}", ex.Message);
                
                return Result.Fail("Не удалось оплатить заказ");
            }
        }

        protected abstract Task<Result<PaymentResult>> PaymentProcessingAsync(Core.Order order, decimal amount);
    }
}
