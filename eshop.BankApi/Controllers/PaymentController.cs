using Microsoft.AspNetCore.Mvc;

namespace eshop.BankApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    [HttpPost]
    public PaymentResponse ProcessPayment([FromBody]PaymentRequest request)
    {
        var isSuccess = new Random().Next() % 3 == 0;

        return new PaymentResponse(isSuccess, isSuccess ? $"Платёж по заказу {request.OrderId} на сумму {request.Amount:F2} принят" : "Упс не повезло :( попробуйте ещё раз");
    }
}