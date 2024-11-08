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

    public BasketController(GetBasketHandler getHandler, AddBasketLineHandler addBasketLineHandler)
    {
        _getHandler = getHandler;
        _addBasketLineHandler = addBasketLineHandler;
    }

    [HttpGet]
    public async Task<ActionResult<Basket>> GetBasketAsync(CancellationToken cancellationToken)
    {
        var result = await _getHandler.GetBasketAsync(cancellationToken);
        if (result.IsFailed)
            return NotFound();

        return Ok(result.Value);
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