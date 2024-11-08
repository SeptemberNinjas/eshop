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
    public async Task<ActionResult<IEnumerable<SaleItemDto>>> GetProductsAsync([FromQuery]int? count, CancellationToken cancellationToken)
    {
        var result = await _handler.GetItemsAsync(ItemTypes.Product, count, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());
        if (!result.Value.Any())
            return NotFound();

        return Ok(result.Value);
    }
    
    [HttpGet("services")]
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