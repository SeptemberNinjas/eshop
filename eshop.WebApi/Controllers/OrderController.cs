using eshop.Application.Order;
using eshop.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly GetOrdersHandler _getOrdersHandler;

    public OrderController(CreateOrderHandler createOrderHandler, GetOrdersHandler getOrdersHandler)
    {
        _createOrderHandler = createOrderHandler;
        _getOrdersHandler = getOrdersHandler;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        var result = await _getOrdersHandler.GetOrdersAsync(cancellationToken);
        if (result.IsFailed)
            return NotFound();

        return Ok(result.Value);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<string>> CreateOrderAsync(CancellationToken cancellationToken)
    {
        var customer = User.Identity?.Name!;
        var result = await _createOrderHandler.CreateOrderAsync(customer, cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());

        return Ok(result.ToString());
    }
}