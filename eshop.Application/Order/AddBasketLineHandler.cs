using eshop.Core;
using eshop.DAL;
using FluentResults;

namespace eshop.Application.Order
{
    public class AddBasketLineHandler
    {
        private readonly RepositoryFactory _repositoryFactory;

        public AddBasketLineHandler(RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<Result> AddLineAsync(string customer, int itemId, int count,
            CancellationToken cancellationToken)
        {
            try
            {
                var basketRepository = _repositoryFactory.CreateBasketRepository();
                
                var baskets = await basketRepository.GetAllAsync(cancellationToken);
                var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
                if (customerBasket is null)
                {
                    await basketRepository.InsertAsync(new Basket(customer), cancellationToken);
                    customerBasket = (await basketRepository.GetAllAsync(cancellationToken))
                        .FirstOrDefault(b => b.Customer == customer);
                }
                if (customerBasket is null)
                    return Result.Fail("Корзина не найдена");

                var itemsRepository = _repositoryFactory.CreateSaleItemRepository();
                var item = await itemsRepository.GetByIdAsync(itemId, cancellationToken);
                if (item is null)
                    return Result.Fail("Товар или услуга не найдены");
          
                var result = item switch
                {
                    Product product => await AddLineAsync(product, count, customerBasket, basketRepository),
                    Service service => AddLine(service, customerBasket),
                    _ => Result.Fail("Неизвестный тип товарной единицы")
                };

                if (result.IsSuccess)
                    await basketRepository.UpdateAsync(result.Value, cancellationToken);

                return result.ToResult();
            }
            catch (Exception ex)
            {
                return Result.Fail("Не удалось получить корзину")
                    .WithError(ex.Message)
                    .WithError(ex.StackTrace);
            }
        }
      
        private static async Task<Result<Basket>> AddLineAsync(Product product, int requestedCount, Basket currentBasket, IRepository<Basket> repository)
        {
           if (requestedCount < 1)
                return Result.Fail("Запрашиваемое количество товара должно быть больше 0");

            // Вычисляем доступные остатки с учетом всех корзин
            var productsInBaskets = (await repository.GetAllAsync())
                .SelectMany(b => b.Lines)
                .Where(p => p.ItemType is ItemTypes.Product && p.ItemId == product.Id)
                .Sum(p => p.Count);
            var remainsWithBaskets = product.Stock - productsInBaskets;
        
            if (remainsWithBaskets < requestedCount)
                return Result.Fail($"Нельзя добавить товар в корзину, недостаточно остатков.{Environment.NewLine}" +
                       $"Имеется {product.Stock} из них в корзине {productsInBaskets}, требуется {requestedCount}");

            
            if (IsLineExists(product, currentBasket.Lines, out var line))
                line.Count += requestedCount;
            else
                currentBasket.AddLine(product, requestedCount);

            return Result.Ok(currentBasket)
                .WithSuccess($"В корзину добавлено {requestedCount} единиц товара \'{product.Name}\'");
        }

        private static Result<Basket> AddLine(Service service, Basket currentBasket)
        {
            if (IsLineExists(service, currentBasket.Lines, out _) && service.OnlyOneItem)
                return Result.Fail($"Ошибка при добавлении услуги. Услуга \'{service.Name}\' уже добавлена в корзину");

            currentBasket.AddLine(service);
           
            return Result.Ok(currentBasket)
                .WithSuccess($"В корзину добавлена услуга \'{service.Name}\'");
        }
        
        private static bool IsLineExists(SaleItem saleItem, IEnumerable<ItemsListLine> lines, out ItemsListLine line)
        {
            foreach (var ln in lines)
            {
                if (ln.ItemType != saleItem.ItemType || ln.ItemId != saleItem.Id)
                    continue;
                line = ln;
                return true;
            }

            line = null!;
            return false;
        }
    }
}