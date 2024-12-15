using eshop.Application.Payment;
using eshop.WebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentRequest = eshop.WebApi.Requests.PaymentRequest;

namespace eshop.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<PaymentResponse>> PayAsync(PaymentRequest request, CancellationToken cancellationToken)
        {
            var  result = request.PaymentType == 1 ?
                await _payByCashHandler.PayHandler(request.OrderId, request.Amount, cancellationToken) :
                await _payOrderByCashlessHandler.PayHandler(request.OrderId, request.Amount, cancellationToken);

            var isSuccess = result.IsSuccess && result.Value.IsSuccess;
            var message = result.IsSuccess ? result.Value.Message : result.ToString();

            return Ok(new PaymentResponse(isSuccess, message));
        }
    }
}
