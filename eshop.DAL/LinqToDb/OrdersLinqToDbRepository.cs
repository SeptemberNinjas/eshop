using eshop.Core;
using eshop.DAL.LinqToDb.Schema;
using LinqToDB;
using LinqToDB.Data;

namespace eshop.DAL.LinqToDb
{
    public class OrdersLinqToDbRepository : IRepository<Order>
    {
        private readonly LinqToDbContext _context;

        public OrdersLinqToDbRepository(LinqToDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .LoadWith(b => b.Lines)
                .ThenLoad(l => l.Item)
                .Select(r => MapOrder(r))
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders.CountAsync(cancellationToken);
        }

        public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _context.Orders
                .LoadWith(b => b.Lines)
                .ThenLoad(l => l.Item)
                .Where(r => r.Id == id)
                .Select(r => MapOrder(r))
                .ToListAsync(cancellationToken); // При наличии ассоциации один к многим метод FirstOrDefaultAsync не работает

            return result.FirstOrDefault();
        }

        public async Task UpdateAsync(Order item, CancellationToken cancellationToken = default)
        {
            await _context.Orders
                .Where(r => r.Id == item.Id)
                .Set(r => r.Status, (int)item.Status)
                .UpdateAsync(cancellationToken);
        }

        public async Task<int> InsertAsync(Order item, CancellationToken cancellationToken = default)
        {
            DataConnectionTransaction? transaction = null;
            if (_context.Transaction is null)
                transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                var identity = await _context.Orders.InsertWithIdentityAsync(() => new OrderRow { Status = (int)item.Status }, cancellationToken);
                var orderId = (int)identity;

                var lines = item.Lines
                    .Select(l => new OrderLine
                    {
                        OrderId = orderId,
                        ItemId = l.ItemId,
                        Count = l.Count
                    });

                await _context.OrderLines.BulkCopyAsync(lines, cancellationToken);

                if (transaction is not null)
                    await transaction.CommitAsync(cancellationToken);

                return orderId;
            }
            catch
            {
                if (transaction is not null)
                    await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (transaction is not null)
                    await transaction.DisposeAsync();
            }
        }

        private static ItemsListLine MapOrderLine(OrderLine line)
        {
            SaleItem item = (ItemTypes)line.Item!.Type switch
            {
                ItemTypes.Product => new Product(line.Item.Id, line.Item.Name, line.Item.Price,
                    line.Item.Stock?.Amount ?? 0),
                ItemTypes.Service => new Service(line.Item.Id, line.Item.Name, line.Item.Price),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new ItemsListLine(item, line.Count);
        }

        internal static Order MapOrder(OrderRow order)
        {
            return new Order(
                order.Id,
                (OrderStatus)order.Status,
                order.Lines!.Select(MapOrderLine));
        }
    }
}