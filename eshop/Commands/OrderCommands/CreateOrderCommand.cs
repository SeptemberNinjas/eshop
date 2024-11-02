using eshop.Core;
using eshop.DAL;

namespace eshop.Commands.OrderCommands;

/// <summary>
/// Команда создания заказа
/// </summary>
public class CreateOrderCommand : IEshopCommand
{
    private readonly IRepository<Basket> _basket;
    private readonly IRepository<Order> _orders;
    private readonly IRepository<Stock> _stock;

    /// <inheritdoc cref="CreateOrderCommand"/>
    public CreateOrderCommand(RepositoryFactory repositoryFactory)
    {
        _basket = repositoryFactory.CreateBasketRepository();
        _orders = repositoryFactory.CreateOrdersRepository();
        _stock = repositoryFactory.CreateStockRepository();
    }
   
    public const string Info = "Создать заказ из текущей корзины";
    
    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result { get; private set; }

    /// <inheritdoc />
    public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        var currentBasket = await _basket.GetByIdAsync(default);
        var order = currentBasket?.CreateOrderFromBasket();
        if (order is null)
        {
            Result = "Ошибка при создании заказа. Корзина пуста";
            return;
        }
        
        var orderedProductsWithCount = order.Lines
            .Where(l => l.ItemType is ItemTypes.Product)
            .Join(await _stock.GetAllAsync(),
                orderLine => orderLine.ItemId, 
                stock => stock.ItemId,
                (orderLine, stock) => (stock, orderLine.Count));
                
        var id = await _orders.InsertAsync(order);
        await _basket.UpdateAsync(currentBasket!);
        foreach (var (stock, count) in orderedProductsWithCount)
        {
            if (stock.Amount - count < 0)
                throw new ApplicationException("Недостаточно товара");

            stock.Amount -= count;
            await _stock.UpdateAsync(stock);
        }

        Result = $"Создан заказ {id}";
    }
}