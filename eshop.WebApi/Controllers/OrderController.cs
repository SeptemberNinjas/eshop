using eshop.Application.Order;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly CreateOrderHandler _createOrderHandler;

    public OrderController(CreateOrderHandler createOrderHandler)
    {
        _createOrderHandler = createOrderHandler;
    }
    
    [HttpPost]
    public async Task<ActionResult<string>> CreateOrderAsync(CancellationToken cancellationToken)
    {
        var result = await _createOrderHandler.CreateOrderAsync(cancellationToken);
        if (result.IsFailed)
            return BadRequest(result.ToString());

        return Ok(result.ToString());
    }
}