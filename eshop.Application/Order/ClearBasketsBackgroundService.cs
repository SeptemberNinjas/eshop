using eshop.Core;
using eshop.DAL.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eshop.Application.Order;

public class ClearBasketsBackgroundService : BackgroundService
{
    private const int DelayTimeoutMinutes = 1;
    private const int MaxBasketLifetimeHours = 1;

    private readonly ILogger<ClearBasketsBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ClearBasketsBackgroundService(ILogger<ClearBasketsBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Ошибка при выполнении фоновой операции очистки корзин");
            }

            await Task.Delay(TimeSpan.FromMinutes(DelayTimeoutMinutes), cancellationToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var clearedBaskets = new List<Basket>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Basket>>();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        try
        {
            await context.BeginTransactionAsync(cancellationToken);
            var baskets = await repository.GetAllAsync(cancellationToken);
            foreach (var basket in baskets)
            {
                if (basket.Lines.Count == 0 ||
                    DateTime.UtcNow - basket.LastUpdate < TimeSpan.FromHours(MaxBasketLifetimeHours))
                    continue;
                basket.Clear();
                await repository.UpdateAsync(basket, cancellationToken);
                clearedBaskets.Add(basket);
            }

            await context.CommitTransactionAsync(cancellationToken);
            if (clearedBaskets.Count > 0)
                _logger.LogInformation("Очищены корзины покупателей: {customers}",
                    string.Join(", ", clearedBaskets.Select(b => b.Customer)));
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}