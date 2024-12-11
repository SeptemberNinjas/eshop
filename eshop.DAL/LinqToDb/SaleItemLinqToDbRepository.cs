using eshop.Core;
using eshop.DAL.LinqToDb.Schema;
using LinqToDB;

namespace eshop.DAL.LinqToDb
{
    public class SaleItemLinqToDbRepository : IReadOnlyRepository<SaleItem>
    {
        private readonly LinqToDbContext _context;

        public SaleItemLinqToDbRepository(LinqToDbContext context)
        {
            _context = context;
        }

        private static SaleItem GetSaleItem(Catalog catalog, Schema.Stock stock)
        {
            var type = (ItemTypes)catalog.Type;

            return type switch
            {
                ItemTypes.Product => new Product(catalog.Id, catalog.Name, catalog.Price, stock.Amount),
                ItemTypes.Service => new Service(catalog.Id, catalog.Name, catalog.Price),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public async Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await _context.Catalog
                .LeftJoin(_context.Stock.AsQueryable(),
                    (c, s) => c.Id == s.Id,
                    (c, s) => GetSaleItem(c, s))
                .ToListAsync(cancellationToken);

            return result;
        }

        public async Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _context.Catalog
                .Where(c => c.Id == id)
                .LeftJoin(_context.Stock.AsQueryable(),
                    (c, s) => c.Id == s.Id,
                    (c, s) => GetSaleItem(c, s))
            .FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Catalog.CountAsync(cancellationToken);
        }
    }
}
