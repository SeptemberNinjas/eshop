using eshop.Application.Order;
using eshop.Core;
using eshop.WebApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BasketController : ControllerBase
{
    private readonly GetBasketHandler _getHandler;
    private readonly AddBasketLineHandler _addBasketLineHandler;
    private readonly ClearBasketHandler _clearBasketHandler;

    public BasketController(
        GetBasketHandler getHandler, 
        AddBasketLineHandler addBasketLineHandler, 
        ClearBasketHandler clearBasketHandler)
    {
        _getHandler = getHandler;
        _addBasketLineHandler = addBasketLineHandler;
        _clearBasketHandler = clearBasketHandler;
    }

    [HttpGet]
    public async Task<ActionResult<Basket>> GetBasketAsync(CancellationToken cancellationToken)
    {
        var result = await _getHandler.GetBasketAsync(cancellationToken);
        if (result.IsFailed)
            return NotFound();

        return Ok(result.Value);
    }
    
    [HttpDelete]
    public async Task<ActionResult<Basket>> ClearBasketAsync(CancellationToken cancellationToken)
    {
        var result = await _clearBasketHandler.ClearBasketAsync(cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());

        return Ok(result.ToString());
    }
    
    [HttpPatch("line")]
    public async Task<ActionResult<string>> AddLineAsync([FromBody]AddLineRequest request, CancellationToken cancellationToken)
    {
        var result = await _addBasketLineHandler.AddLineAsync(request.ItemId, request.CountToAdd ?? 1, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());

        return Ok(result.ToString());
    }
}