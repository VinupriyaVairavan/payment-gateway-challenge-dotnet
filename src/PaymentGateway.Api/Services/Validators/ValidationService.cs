using FluentValidation;
using PaymentGateway.Models.Requests;

namespace PaymentGateway.Api.Services.Validators;

public class ValidationService : IValidationService
{
    private readonly IValidator<PostPaymentRequest> _paymentRequestValidator;

    public ValidationService(IValidator<PostPaymentRequest> paymentRequestValidator)
    {
        _paymentRequestValidator = paymentRequestValidator;
    }

    public List<string> ValidatePaymentRequest(PostPaymentRequest paymentRequest)
    {
        var validationResults = _paymentRequestValidator.Validate(paymentRequest);

        var errors = validationResults.Errors
            .Select(e => e.ErrorMessage)
            .ToList();

        return errors;
    }
}