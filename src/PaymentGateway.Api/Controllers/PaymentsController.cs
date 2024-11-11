using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using PaymentGateway.Models.Requests;
using PaymentGateway.Models.Responses;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService)
    : Controller
{
    [HttpPost()]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PostPaymentResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostPaymentResponse?>> PostPaymentAsync(PostPaymentRequest paymentRequest)
    {
        var response = await paymentService.ProcessPaymentAsync(paymentRequest);
        
        if(response == null)
            return new NotFoundObjectResult(response);
        
        if(response.Status != PaymentStatus.Authorized.ToString())  
            return new BadRequestObjectResult(response);
        
        return new OkObjectResult(response);
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GetPaymentResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = await paymentService.GetPaymentAsync(id);
        
        if(payment == null)
            return new NotFoundObjectResult(payment);
        
        return new OkObjectResult(payment);
    }
}