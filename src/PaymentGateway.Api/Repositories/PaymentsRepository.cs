using PaymentGateway.Api.Entities;

namespace PaymentGateway.Api.Repositories;

public class PaymentsRepository : IPaymentRepository
{
    private List<Payment> _payments = new();
    
    public async Task AddAsync(Payment payment)
    {
        await Task.Run(() => _payments.Add(payment));
    }

    public async Task<Payment?> GetAsync(Guid id)
    {
        return await Task.Run(() => _payments.FirstOrDefault(p => p.Id == id));
    }
}