namespace eshop.Core;

public interface ICustomerOrdersRepository
{
    Task LinkOrderToCustomerAsync(string customer, int orderId, CancellationToken cancellationToken);
    
    Task<List<Order>> GetCustomerOrdersAsync(string customer, CancellationToken cancellationToken);
}