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
    private readonly IRepository<Product> _products;

    /// <inheritdoc cref="CreateOrderCommand"/>
    public CreateOrderCommand(RepositoryFactory repositoryFactory)
    {
        _basket = repositoryFactory.CreateBasketRepository();
        _orders = repositoryFactory.CreateOrdersRepository();
        _products = repositoryFactory.CreateProductRepository();
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
            .Join(_products.GetAll(),
                orderLine => orderLine.ItemId, 
                repoProduct => repoProduct.Id,
                (orderLine, repoProduct) => (repoProduct, orderLine.Count));
                
        var id = _orders.Insert(order);
        _basket.Update(currentBasket!);
        foreach (var (product, count) in orderedProductsWithCount)
        {
            if (product.Stock - count < 0)
                throw new ApplicationException("Недостаточно товара");

            product.Stock -= count;
            _products.Update(product);
        }

        Result = $"Создан заказ {id}";
    }
}