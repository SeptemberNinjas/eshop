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

        public async Task<Result<Basket>> GetBasketAsync(CancellationToken cancellationToken)
        {
            try
            {
                var repository = _repositoryFactory.CreateBasketRepository();
                var currentBasket = (await repository.GetAllAsync(cancellationToken)).FirstOrDefault();
                if (currentBasket is null || currentBasket.Lines.Count == 0)
                    return Result.Fail("Корзина не найдена");
                
                return Result.Ok(currentBasket);
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