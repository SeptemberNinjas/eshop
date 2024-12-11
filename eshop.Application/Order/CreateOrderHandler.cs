using eshop.Core;
using eshop.Core.Cache;
using eshop.DAL.Database;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class CreateOrderHandler
{
    private readonly DatabaseContext _databaseContext;
    private readonly IRepository<Core.Order> _ordersRepository;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<Stock> _stockRepository;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly CacheKeysStorage _keysStorage;

    public CreateOrderHandler(
        DatabaseContext databaseContext,
        IRepository<Core.Order> ordersRepository,
        IRepository<Basket> basketRepository,
        IRepository<Stock> stockRepository,
        ILogger<CreateOrderHandler> logger,
        CacheKeysStorage keysStorage)
    {
        _databaseContext = databaseContext;
        _ordersRepository = ordersRepository;
        _basketRepository = basketRepository;
        _logger = logger;
        _keysStorage = keysStorage;
        _stockRepository = stockRepository;
    }

    public async Task<Result> CreateOrderAsync(string customer, CancellationToken cancellationToken)
    {
        try
        {
            await _databaseContext.BeginTransactionAsync(cancellationToken);
            var currentBasket = (await _basketRepository.GetAllAsync(cancellationToken))
                .FirstOrDefault(b => b.Customer == customer);
            if (currentBasket is null || currentBasket.Lines.Count == 0)
                return Result.Fail("Корзина не найдена");
  
            var order = currentBasket.CreateOrderFromBasket();
            if (order is null)
                return Result.Fail("Ошибка при создании заказа. Корзина пуста");
            
            var orderedProductsWithCount = order.Lines
                .Where(l => l.ItemType is ItemTypes.Product)
                .Join(await _stockRepository.GetAllAsync(cancellationToken),
                    orderLine => orderLine.ItemId, 
                    stock => stock.ItemId,
                    (orderLine, stock) => (stock, orderLine.Count));
                
            // Тут происходят взаимосвязанные изменения в трёх репозиториях
            // Хорошее место, чтобы показать транзакцию
            var id = await _ordersRepository.InsertAsync(order, cancellationToken);
            await _basketRepository.UpdateAsync(currentBasket, cancellationToken);
            foreach (var (stock, count) in orderedProductsWithCount)
            {
                if (stock.Amount - count < 0)
                {
                    await _databaseContext.RollbackTransactionAsync(cancellationToken);
                    return Result.Fail("Недостаточно товара");
                }

                stock.Amount -= count;
                await _stockRepository.UpdateAsync(stock, cancellationToken);
            }

            await _databaseContext.CommitTransactionAsync(cancellationToken);
            _keysStorage.RemoveGroupCache(CacheKeysStorage.SaleItemsGroup);
            
            return Result.Ok()
                .WithSuccess($"Создан заказ {id}");
        }
        catch (Exception ex)
        {
            await _databaseContext.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Ошибка при создании заказа. {message}", ex.Message);
            
            return Result.Fail("Не удалось создать заказ");
        }
    }
}