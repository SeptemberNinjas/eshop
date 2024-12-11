using eshop.Application.SaleItems;
using eshop.Core;
using eshop.Core.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CatalogController : ControllerBase
{
    private readonly GetSaleItemHandler _handler;
    private readonly IMemoryCache _memoryCache;
    private readonly CacheKeysStorage _keysStorage;

    public CatalogController(GetSaleItemHandler handler, IMemoryCache memoryCache, CacheKeysStorage keysStorage)
    {
        _handler = handler;
        _memoryCache = memoryCache;
        _keysStorage = keysStorage;
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetProductsAsync([FromQuery]int? count, CancellationToken cancellationToken)
    {
        var cacheKey = $"products_{count}";
        var result = await _memoryCache.GetOrCreateAsync(cacheKey, async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);

            var result = await _handler.GetItemsAsync(ItemTypes.Product, count, cancellationToken);
            if (result.IsFailed)
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1);

            _keysStorage.RegisterGroupKey(CacheKeysStorage.SaleItemsGroup, cacheKey);
            return result;
        });
        
        if (result is null)
            return Problem();
        
        if (result.IsFailed)
            return BadRequest(result.ToString());
        
        if (!result.Value.Any())
            return NotFound();

        return Ok(result.Value);
    }
    
    [HttpGet("services")]
    [ResponseCache(VaryByQueryKeys = ["count"], Duration = 30)]
    public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetServicesAsync([FromQuery]int? count, CancellationToken cancellationToken)
    {
        var result = await _handler.GetItemsAsync(ItemTypes.Service, count, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());
        if (!result.Value.Any())
            return NotFound();

        return Ok(result.Value);
    }
}