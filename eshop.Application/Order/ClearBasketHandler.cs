using eshop.DAL;
using FluentResults;

namespace eshop.Application.Order;

public class ClearBasketHandler
{
    private readonly RepositoryFactory _repositoryFactory;

    public ClearBasketHandler(RepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task<Result> ClearBasketAsync(string customer, CancellationToken cancellationToken)
    {
        try
        {
            var repository = _repositoryFactory.CreateBasketRepository();
            var baskets = await repository.GetAllAsync(cancellationToken);
            var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
            if (customerBasket is null)
                return Result.Fail("Корзина не найдена");
            
            customerBasket.Clear();
            await repository.UpdateAsync(customerBasket, cancellationToken);
                
            return Result.Ok()
                .WithSuccess("Корзина очищена");
        }
        catch (Exception ex)
        {
            return Result.Fail("Не удалось очистить корзину")
                .WithError(ex.Message)
                .WithError(ex.StackTrace);
        }
    }
}