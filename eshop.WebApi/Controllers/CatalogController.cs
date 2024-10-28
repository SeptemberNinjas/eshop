using eshop.Application.SaleItems;
using eshop.Core;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CatalogController : ControllerBase
{
    private readonly GetSaleItemHandler _handler;

    public CatalogController(GetSaleItemHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("products")]
    public async Task<IEnumerable<SaleItemDto>> GetProductsAsync([FromQuery]int? count)
    {
        return await _handler.GetItemsAsync(ItemTypes.Product, count);
    }
    
    [HttpGet("services")]
    public async Task<IEnumerable<SaleItemDto>> GetServicesAsync([FromQuery]int? count)
    {
        return await _handler.GetItemsAsync(ItemTypes.Service, count);
    }
}