using eshop.Application.Payment;
using eshop.WebApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOrderByCashHandler _payByCashHandler;
        private readonly PayOrderByCashlessHandler _payOrderByCashlessHandler;

        public PaymentController(
            PayOrderByCashHandler payByCashHandler,
            PayOrderByCashlessHandler payOrderByCashlessHandler)
        {
            _payByCashHandler = payByCashHandler;
            _payOrderByCashlessHandler = payOrderByCashlessHandler;
        }

        [HttpPatch]
        public async Task<ActionResult<string>> PayAsync(PaymentRequest request, CancellationToken cancellationToken)
        {
            var  result = request.PaymentType == 1 ?
                await _payByCashHandler.PayHandler(request.OrderId, request.Amount, cancellationToken) :
                await _payOrderByCashlessHandler.PayHandler(request.OrderId, request.Amount, cancellationToken);

            if (result.IsFailed)
                return BadRequest(result.ToString());

            return Ok(result.ToString());
        }
    }
}
