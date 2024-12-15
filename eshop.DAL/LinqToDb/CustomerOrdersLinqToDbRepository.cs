using eshop.Core;
using eshop.DAL.LinqToDb.Schema;
using LinqToDB;

namespace eshop.DAL.LinqToDb
{
    public class CustomerOrdersLinqToDbRepository : ICustomerOrdersRepository
    {
        private readonly LinqToDbContext _context;

        public CustomerOrdersLinqToDbRepository(LinqToDbContext context)
        {
            _context = context;
        }

        public async Task LinkOrderToCustomerAsync(string customer, int orderId, CancellationToken cancellationToken)
        {
            await _context.CustomerOrders.InsertAsync(() => new CustomerOrders
            {
                Customer = customer,
                OrderId = orderId
            }, token: cancellationToken);
        }

        public async Task<List<Order>> GetCustomerOrdersAsync(string customer, CancellationToken cancellationToken)
        {
            return await _context.CustomerOrders
                .LoadWith(r => r.Orders)
                .ThenLoad(o => o.Lines)
                .ThenLoad(l => l.Item)
                .Where(r => r.Customer == customer)
                .SelectMany(r => r.Orders!)
                .Select(r => OrdersLinqToDbRepository.MapOrder(r))
                .ToListAsync(cancellationToken);
        }
    }
}