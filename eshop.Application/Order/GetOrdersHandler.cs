using eshop.Core;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class GetOrdersHandler
{
    private readonly IRepository<Core.Order> _ordersRepository;
    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(IRepository<Core.Order> ordersRepository, ILogger<GetOrdersHandler> logger)
    {
        _ordersRepository = ordersRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Core.Order>>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _ordersRepository.GetAllAsync(cancellationToken);
            return Result.Ok(orders.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказов. {message}", ex.Message);
            
            return Result.Fail("Не удалось получить список заказов");
        }
    }
}