namespace PaymentGateway.Api.Settings;

public class PaymentProviderSetting
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = null!;
    public int TimeOutSeconds { get; set; } 
}