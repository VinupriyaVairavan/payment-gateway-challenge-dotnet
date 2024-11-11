using System.Text.Json.Serialization;

namespace PaymentGateway.Models.Requests;

public class PostPaymentProviderRequest
{
    public Guid Id { get; set; }
    public string card_number { get; set; } = string.Empty;
    public string expiry_date { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Cvv { get; set; }= string.Empty;
}