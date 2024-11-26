using eshop.Core;
using eshop.DAL;
using FluentResults;

namespace eshop.Application.Order
{
    public class GetBasketHandler
    {
        private readonly RepositoryFactory _repositoryFactory;

        public GetBasketHandler(RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
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
                return Result.Fail("Не удалось получить корзину")
                    .WithError(ex.Message)
                    .WithError(ex.StackTrace);
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