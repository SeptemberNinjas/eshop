using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace eshop.Core.Cache;

public class CacheKeysStorage
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _groupKeys = new();
    private readonly IMemoryCache _memoryCache;

    public const string SaleItemsGroup = "SaleItemsGroup";

    public CacheKeysStorage(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void RegisterGroupKey(string groupName, string key)
    {
        _groupKeys.AddOrUpdate(
            groupName,
            _ => [key],
            (_, keys) =>
            {
                var result = keys.ToHashSet();
                result.Add(key);
                return result;
            });
    }

    public void RemoveGroupCache(string groupName)
    {
        if (!_groupKeys.TryRemove(groupName, out var keys))
            return;

        foreach (var key in keys)
        {
            _memoryCache.Remove(key);
        }
    }
}