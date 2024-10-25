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
    public void Execute(string[]? args)
    {
        var currentBasket = _basket.GetById(default);
        var order = currentBasket?.CreateOrderFromBasket();
        if (order is null)
        {
            Result = "Ошибка при создании заказа. Корзина пуста";
            return;
        }
        
        var orderedProductsWithCount = order.Lines
            .Where(l => l.ItemType is ItemTypes.Product)
            .Join(_stock.GetAll(),
                orderLine => orderLine.ItemId, 
                stock => stock.ItemId,
                (orderLine, stock) => (stock, orderLine.Count));
                
        var id = _orders.Insert(order);
        _basket.Update(currentBasket!);
        foreach (var (stock, count) in orderedProductsWithCount)
        {
            if (stock.Amount - count < 0)
                throw new ApplicationException("Недостаточно товара");

            stock.Amount -= count;
            _stock.Update(stock);
        }

        Result = $"Создан заказ {id}";
    }
}