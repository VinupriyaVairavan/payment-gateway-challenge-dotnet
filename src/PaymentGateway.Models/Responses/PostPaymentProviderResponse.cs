using System.Text.Json.Serialization;

namespace PaymentGateway.Models.Responses;

public class PostPaymentProviderResponse
{
    public bool Authorized { get; set; }
    
    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; set; }
}