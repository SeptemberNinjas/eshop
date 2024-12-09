using eshop.Core;
using eshop.DAL;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class CreateOrderHandler
{
    private readonly RepositoryFactory _repositoryFactory;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(RepositoryFactory repositoryFactory, ILogger<CreateOrderHandler> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    public async Task<Result> CreateOrderAsync(CancellationToken cancellationToken)
    {
        try
        {
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

            return Result.Ok()
                .WithSuccess($"Создан заказ {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании заказа. {message}", ex.Message);
            
            return Result.Fail("Не удалось создать заказ");
        }
    }
}