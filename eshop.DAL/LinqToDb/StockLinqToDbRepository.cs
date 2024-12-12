using eshop.Core;
using LinqToDB;
using Stock = eshop.Core.Stock;

namespace eshop.DAL.LinqToDb;

public class StockLinqToDbRepository : IRepository<Stock>
{
    private readonly LinqToDbContext _context;

    public StockLinqToDbRepository(LinqToDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stock
            .Select(r => MapStock(r))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Baskets.CountAsync(cancellationToken);
    }

    public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Stock
            .Where(r => r.Id == id)
            .Select(r => MapStock(r))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(Stock item, CancellationToken cancellationToken = default)
    {
        await _context.Stock
            .Where(r => r.Id == item.ItemId)
            .Set(r => r.Amount, item.Amount)
            .UpdateAsync(cancellationToken);
    }

    public async Task<int> InsertAsync(Stock item, CancellationToken cancellationToken = default)
    {
        var row = new Schema.Stock
        {
            Id = item.ItemId,
            Amount = item.Amount
        };
        var result = await _context.Stock.InsertWithOutputAsync(row, cancellationToken);
            
        return result.Id;
    }
        
    private static Stock MapStock(Schema.Stock stock)
    {
        return new Stock
        {
            ItemId = stock.Id,
            Amount = stock.Amount
        };
    }
}