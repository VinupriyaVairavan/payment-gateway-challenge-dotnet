using FluentValidation;

using PaymentGateway.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PaymentRequestValidator: AbstractValidator<PostPaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.CardNumberLastFour)
            .NotEmpty().WithMessage("Card number is required");
            //.CreditCard().WithMessage("Invalid card number");

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required")
            .GreaterThanOrEqualTo(1).WithMessage("Amount must be at least a 1 or more");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12");

        RuleFor(x => x.ExpiryYear)
            .GreaterThanOrEqualTo(DateTime.Now.Year).WithMessage("Expiry year must be current or future year");
        
        RuleFor(x => new { x.ExpiryMonth, x.ExpiryYear })
            .Must(x => IsExpirationDateValid(x.ExpiryMonth, x.ExpiryYear))
            .WithMessage("The expiration date must be in the future.");
        
        RuleFor(x => x.Currency)
            .Must(x => IsValidCurrencyCode(x))
            .WithMessage("Invalid currency code");
    }
    
    private bool IsExpirationDateValid(int expiryMonth, int expiryYear)
    {
        // Validate that the month is between 1 and 12
        if (expiryMonth < 1 || expiryMonth > 12)
            return false;

        // Get today's date
        var currentDate = DateTime.Now;

        // Create a date object from the expiry month and year
        var expirationDate = new DateTime(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1);

        // Compare with current date
        return expirationDate > currentDate;
    }
    
    private static readonly HashSet<string> _validCurrencyCodes = new HashSet<string>
    {
        "USD", "EUR", "JPY", "GBP" // Add more codes as needed
    };
    
    private bool IsValidCurrencyCode(string code)
    {
        return !string.IsNullOrWhiteSpace(code) && _validCurrencyCodes.Contains(code.ToUpper());
    }
}