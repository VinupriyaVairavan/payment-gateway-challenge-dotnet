
namespace PaymentGateway.Models.Responses;

public class PostPaymentResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
    public string? AuthorizationCode { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public string CardNumberLastFour { get; set; } = null!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Amount { get; set; }
}
