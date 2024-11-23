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

        public async Task<Result<Basket>> GetBasketAsync(string customer, CancellationToken cancellationToken)
        {
            try
            {
                var repository = _repositoryFactory.CreateBasketRepository();
                var baskets = await repository.GetAllAsync(cancellationToken);
                var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
                if (customerBasket is null)
                    return Result.Fail($"Корзина покупателя с логином {customer} не найдена");
                
                return Result.Ok(customerBasket);
            }
            catch (Exception ex)
            {
                return Result.Fail("Не удалось получить корзину")
                    .WithError(ex.Message)
                    .WithError(ex.StackTrace);
            }
        }
    }
}