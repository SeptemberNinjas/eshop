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

    public async Task<Result> ClearBasketAsync(CancellationToken cancellationToken)
    {
        try
        {
            var repository = _repositoryFactory.CreateBasketRepository();
            var currentBasket = (await repository.GetAllAsync(cancellationToken)).FirstOrDefault();
            if (currentBasket is null)
                return Result.Fail("Корзина не найдена");
            
            currentBasket.Clear();
            await repository.UpdateAsync(currentBasket, cancellationToken);
                
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