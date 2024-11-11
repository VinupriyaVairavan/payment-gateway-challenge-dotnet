using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.ErrorHandler;

[ApiController]
[Route("error")]
public class ErrorController : ControllerBase
{
    [Route("handle")]
    [HttpGet]
    public IActionResult Handle()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context?.Error;

        var response = new ErrorResponse
        {
            StatusCode = 500,
            Message = "An error occurred while processing your request.",
            Details = exception?.Message
        };

        return StatusCode(500, response);
    }

    [Route("not-found")]
    [HttpGet]
    public IActionResult NotFoundHandler()
    {
        return NotFound(new ErrorResponse
        {
            StatusCode = 404,
            Message = "The requested resource was not found."
        });
    }
}