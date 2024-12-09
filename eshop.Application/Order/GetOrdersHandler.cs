using eshop.DAL;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class GetOrdersHandler
{
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(RepositoryFactory repositoryFactory, ILogger<GetOrdersHandler> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Core.Order>>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var repository = _repositoryFactory.CreateOrdersRepository();
            var orders = await repository.GetAllAsync(cancellationToken);
            return Result.Ok(orders.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказов. {message}", ex.Message);
            
            return Result.Fail("Не удалось получить список заказов");
        }
    }
}