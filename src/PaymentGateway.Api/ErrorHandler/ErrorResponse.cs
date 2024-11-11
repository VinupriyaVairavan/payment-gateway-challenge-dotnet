namespace PaymentGateway.Api.ErrorHandler;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
}