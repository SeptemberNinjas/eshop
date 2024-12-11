using eshop.Core;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class ClearBasketHandler
{
    private readonly IRepository<Basket> _basketRepository;
    private readonly ILogger<ClearBasketHandler> _logger;

    public ClearBasketHandler(IRepository<Basket> basketRepository, ILogger<ClearBasketHandler> logger)
    {
        _basketRepository = basketRepository;
        _logger = logger;
    }

    public async Task<Result> ClearBasketAsync(string customer, CancellationToken cancellationToken)
    {
        try
        {
            var baskets = await _basketRepository.GetAllAsync(cancellationToken);
            var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
            if (customerBasket is null)
                return Result.Fail("Корзина не найдена");
            
            customerBasket.Clear();
            await _basketRepository.UpdateAsync(customerBasket, cancellationToken);
                
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