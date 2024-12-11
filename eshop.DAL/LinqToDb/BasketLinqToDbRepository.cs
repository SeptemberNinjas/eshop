using eshop.Core;
using eshop.DAL.LinqToDb.Schema;
using LinqToDB;
using LinqToDB.Data;

namespace eshop.DAL.LinqToDb
{
    public class BasketLinqToDbRepository : IRepository<Basket>
    {
        private readonly LinqToDbContext _context;

        public BasketLinqToDbRepository(LinqToDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<Basket>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Baskets
                .LoadWith(b => b.Lines)
                .ThenLoad(l => l.Item)
                .Select(r => MapBasket(r))
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Baskets.CountAsync(cancellationToken);
        }

        public async Task<Basket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _context.Baskets
                .LoadWith(b => b.Lines)
                .ThenLoad(l => l.Item)
                .Where(r => r.Id == id)
                .Select(r => MapBasket(r))
                .ToListAsync(cancellationToken); // При наличии ассоциации один к многим метод FirstOrDefaultAsync не работает

            return result.FirstOrDefault();
        }

        public async Task UpdateAsync(Basket item, CancellationToken cancellationToken = default)
        {
            DataConnectionTransaction? transaction = null;
            if (_context.Transaction is null)
                transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                await _context.Baskets
                    .Where(r => r.Id == item.Id)
                    .Set(r => r.Customer, item.Customer)
                    .UpdateAsync(cancellationToken);

                await _context.BasketLines
                    .Where(r => r.BasketId == item.Id)
                    .DeleteAsync(cancellationToken);

                var lines = item.Lines
                    .Select(l => new BasketLine
                    {
                        BasketId = item.Id,
                        ItemId = l.ItemId,
                        Count = l.Count
                    });

                await _context.BasketLines.BulkCopyAsync(lines, cancellationToken);

                if (transaction is not null)
                    await transaction.CommitAsync(cancellationToken);
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

        public async Task<int> InsertAsync(Basket item, CancellationToken cancellationToken = default)
        {
            var id = await _context.Baskets.InsertWithIdentityAsync(() => new BasketRow { Customer = item.Customer }, cancellationToken);
            
           return (int)id;
        }

        private static ItemsListLine MapBasketLine(BasketLine line)
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

        private static Basket MapBasket(BasketRow basket)
        {
            return new Basket(
                basket.Id,
                basket.Lines?.Select(MapBasketLine) ?? [],
                basket.Customer!);
        }
    }
}