using eshop.Core;
using eshop.DAL;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order
{
    public class GetBasketHandler
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly ILogger<GetBasketHandler> _logger;

        public GetBasketHandler(RepositoryFactory repositoryFactory, ILogger<GetBasketHandler> logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
        }

        public async Task<Result<BasketDto>> GetBasketAsync(string customer, CancellationToken cancellationToken)
        {
            try
            {
                var repository = _repositoryFactory.CreateBasketRepository();
                var baskets = await repository.GetAllAsync(cancellationToken);
                var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
                if (customerBasket is null)
                    return Result.Fail($"Корзина покупателя с логином {customer} не найдена");

                return Result.Ok(Map(customerBasket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении корзины. {message}", ex.Message);
                
                return Result.Fail("Не удалось получить корзину");
            }
        }

        private static BasketDto Map(Basket basket)
        {
            var totalSum = basket.Lines.Sum(l => l.SaleItem.Price * l.Count);
            return new BasketDto(basket.Id, basket.Customer, totalSum, basket.Lines
                .Select(l =>
                    new BasketItemDto(l.ItemId, l.ItemType, l.SaleItem.Name, l.Count, l.SaleItem.Price,
                        l.Count * l.SaleItem.Price)));
        }
    }
}