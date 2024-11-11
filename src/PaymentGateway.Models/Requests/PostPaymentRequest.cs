﻿namespace PaymentGateway.Models.Requests;

public class PostPaymentRequest
{
    public Guid Id { get; set; }
    public string CardNumberLastFour { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int Cvv { get; set; }
}