using PaymentGateway.Models.Requests;

namespace PaymentGateway.Api.Services.Validators;

public interface IValidationService
{
    List<string> ValidatePaymentRequest(PostPaymentRequest paymentRequest);
}