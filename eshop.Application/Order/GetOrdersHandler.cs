using eshop.Core;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class GetOrdersHandler
{
    private readonly ICustomerOrdersRepository _ordersRepository;
    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(ICustomerOrdersRepository ordersRepository, ILogger<GetOrdersHandler> logger)
    {
        _ordersRepository = ordersRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<OrderDto>>> GetOrdersAsync(string customer, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _ordersRepository.GetCustomerOrdersAsync(customer, cancellationToken);
            return Result.Ok(orders.Select(Map));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказов. {message}", ex.Message);
            
            return Result.Fail("Не удалось получить список заказов");
        }
    }
    
    private static OrderDto Map(Core.Order order)
    {
        var totalSum = order.Lines.Sum(l => l.SaleItem.Price * l.Count);
        return new OrderDto(order.Id, order.Status, totalSum, order.Lines
            .Select(l =>
                new OrderItemDto(l.ItemId, l.ItemType, l.SaleItem.Name, l.Count, l.SaleItem.Price,
                    l.Count * l.SaleItem.Price)));
    }
}