using PaymentGateway.Api.Entities;

namespace PaymentGateway.Api.Repositories;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetAsync(Guid id);
}