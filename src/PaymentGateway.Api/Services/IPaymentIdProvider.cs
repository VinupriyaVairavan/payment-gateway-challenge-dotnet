using PaymentGateway.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IPaymentIdProvider
{
    Guid Generate(PostPaymentRequest paymentRequest);
}