using eshop.Core;
using eshop.DAL;
using eshop.DAL.Database;
using FluentResults;

namespace eshop.Application.Order;

public class CreateOrderHandler
{
    private readonly DatabaseContext _databaseContext;
    private readonly RepositoryFactory _repositoryFactory;

    public CreateOrderHandler(
        DatabaseContext databaseContext,
        RepositoryFactory repositoryFactory)
    {
        _databaseContext = databaseContext;
        _repositoryFactory = repositoryFactory;
    }

    public async Task<Result> CreateOrderAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _databaseContext.BeginTransactionAsync();

            var basketRepository = _repositoryFactory.CreateBasketRepository();
            var currentBasket = (await basketRepository.GetAllAsync(cancellationToken)).FirstOrDefault();
            if (currentBasket is null || currentBasket.Lines.Count == 0)
                return Result.Fail("Корзина не найдена");
  
            var order = currentBasket.CreateOrderFromBasket();
            if (order is null)
                return Result.Fail("Ошибка при создании заказа. Корзина пуста");

            var stockRepository = _repositoryFactory.CreateStockRepository();
            var orderedProductsWithCount = order.Lines
                .Where(l => l.ItemType is ItemTypes.Product)
                .Join(await stockRepository.GetAllAsync(cancellationToken),
                    orderLine => orderLine.ItemId, 
                    stock => stock.ItemId,
                    (orderLine, stock) => (stock, orderLine.Count));
                
            // Тут происходят взаимосвязанные изменения в трёх репозиториях
            // Хорошее место, чтобы показать транзакцию
            var ordersRepository = _repositoryFactory.CreateOrdersRepository();
            var id = await ordersRepository.InsertAsync(order, cancellationToken);
            await basketRepository.UpdateAsync(currentBasket, cancellationToken);
            foreach (var (stock, count) in orderedProductsWithCount)
            {
                if (stock.Amount - count < 0)
                    return Result.Fail("Недостаточно товара");

                stock.Amount -= count;
                await stockRepository.UpdateAsync(stock, cancellationToken);
            }

            await _databaseContext.CommitTransactionAsync();

            return Result.Ok()
                .WithSuccess($"Создан заказ {id}");
        }
        catch (Exception ex)
        {
            return Result.Fail("Не удалось создать заказ")
                .WithError(ex.Message)
                .WithError(ex.StackTrace);
        }
    }
}