using eshop.DAL.LinqToDb;
using LinqToDB.Data;

namespace eshop.DAL.Database;

/// <summary>
/// Контекст подключения к СУБД
/// </summary>
public class DatabaseContext : IDisposable
{
    private readonly LinqToDbContext? _context;
    private DataConnectionTransaction? _transaction;

    public DatabaseContext(LinqToDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _context?.Dispose();
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_context is not null)
            _transaction = await _context.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
            await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(cancellationToken);
    }
}