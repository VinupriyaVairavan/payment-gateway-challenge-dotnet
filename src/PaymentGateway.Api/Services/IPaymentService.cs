using PaymentGateway.Models.Requests;
using PaymentGateway.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<PostPaymentResponse?> ProcessPaymentAsync(PostPaymentRequest paymentRequest);
    Task<GetPaymentResponse?> GetPaymentAsync(Guid paymentId);
}