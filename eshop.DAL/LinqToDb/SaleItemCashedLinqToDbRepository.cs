using eshop.Core;
using eshop.Core.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace eshop.DAL.LinqToDb
{
    public class SaleItemCashedLinqToDbRepository : IReadOnlyRepository<SaleItem>
    {
        private const int RepositoryCacheLifetimeSeconds = 20;

        private readonly SaleItemLinqToDbRepository _repository;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheKeysStorage _keysStorage;

        public SaleItemCashedLinqToDbRepository(SaleItemLinqToDbRepository repository, IMemoryCache memoryCache, CacheKeysStorage keysStorage)
        {
            _repository = repository;
            _memoryCache = memoryCache;
            _keysStorage = keysStorage;
        }

        public async Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = $"{nameof(SaleItemCashedLinqToDbRepository)}{nameof(GetAllAsync)}";
            return await WrapWithCacheAsync(cacheKey, () => _repository.GetAllAsync(cancellationToken));
        }

        public async Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string cacheKey = $"{nameof(SaleItemCashedLinqToDbRepository)}{nameof(GetByIdAsync)}";
            return await WrapWithCacheAsync(cacheKey, () => _repository.GetByIdAsync(id, cancellationToken));
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = $"{nameof(SaleItemCashedLinqToDbRepository)}{nameof(GetCountAsync)}";
            return await WrapWithCacheAsync(cacheKey, () => _repository.GetCountAsync(cancellationToken));
        }

        private async Task<T> WrapWithCacheAsync<T>(string cacheKey, Func<Task<T>> func)
        {
            var result = await _memoryCache.GetOrCreateAsync(cacheKey,
                async e =>
                {
                    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RepositoryCacheLifetimeSeconds);
                    _keysStorage.RegisterGroupKey(CacheKeysStorage.SaleItemsGroup, cacheKey);
                    return await func();
                });

            return result!;
        }
    }
}