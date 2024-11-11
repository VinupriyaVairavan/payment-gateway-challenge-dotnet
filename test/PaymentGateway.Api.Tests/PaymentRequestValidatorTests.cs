using FluentValidation.Results;
using PaymentGateway.Api.Validators;
using PaymentGateway.Models.Requests;

namespace PaymentGateway.Api.Tests;

public class PaymentRequestValidatorTests
{
    private readonly PaymentRequestValidator _validator = new();

    [Fact]
    public void Validate_CardNumberLastFourIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new PostPaymentRequest { CardNumberLastFour = string.Empty };
        
        // Act
        ValidationResult result = _validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumberLastFour" && e.ErrorMessage == "Card number is required");
    }

    [Fact]
    public void Validate_AmountIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new PostPaymentRequest { Amount = 0 };
        
        // Act
        ValidationResult result = _validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount" && e.ErrorMessage == "Amount is required");
    }

    [Fact]
    public void Validate_AmountIsLessThanOne_ReturnsValidationError()
    {
        // Arrange
        var request = new PostPaymentRequest { Amount = -1 };
        
        // Act
        ValidationResult result = _validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount" && e.ErrorMessage == "Amount must be at least a 1 or more");
    }

    [Fact]
    public void Validate_ExpiryMonthIsOutOfRange_ReturnsValidationError()
    {
        // Arrange
        var request = new PostPaymentRequest { ExpiryMonth = 13 };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryMonth" && e.ErrorMessage == "Expiry month must be between 1 and 12");
    }

    [Fact]
    public void Validate_ExpiryYearIsInThePast_ReturnsValidationError()
    {
        // Arrange
        var request = new PostPaymentRequest { ExpiryYear = DateTime.Now.Year - 1 };
        
        // Act
        ValidationResult result = _validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryYear" && e.ErrorMessage == "Expiry year must be current or future year");
    }

    [Fact]
    public void Validate_ExpiryDateIsInThePast_ReturnsValidationError()
    {
        // Arrange
        var request = new PostPaymentRequest { ExpiryMonth = 1, ExpiryYear = DateTime.Now.Year, Amount = 100, Currency = "GBP", CardNumberLastFour = "1234"};
        
        // Act
        ValidationResult result = _validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "The expiration date must be in the future.");
    }

    [Fact]
    public void Validate_InvalidCurrencyCode_ReturnsValidationError()
    {
        // Arrange
        var request = new PostPaymentRequest { Currency = "INVALID" };
        
        // Act
        ValidationResult result = _validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency" && e.ErrorMessage == "Invalid currency code");
    }
}