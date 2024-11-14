using eshop.DAL;
using FluentResults;

namespace eshop.Application.Order;

public class GetOrdersHandler
{
    private readonly RepositoryFactory _repositoryFactory;

    public GetOrdersHandler(RepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
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
            return Result.Fail("Не удалось получить корзину")
                .WithError(ex.Message)
                .WithError(ex.StackTrace);
        }
    }
}