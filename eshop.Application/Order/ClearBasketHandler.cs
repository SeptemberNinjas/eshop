using eshop.DAL;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class ClearBasketHandler
{
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ILogger<ClearBasketHandler> _logger;

    public ClearBasketHandler(RepositoryFactory repositoryFactory, ILogger<ClearBasketHandler> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
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
            _logger.LogError(ex, "Ошибка при очистке корзины. {message}", ex.Message);
            
            return Result.Fail("Не удалось очистить корзину");
        }
    }
}