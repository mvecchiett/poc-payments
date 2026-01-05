using Payments.Shared.Models;

namespace Payments.Application.Services;

public interface IPaymentIntentService
{
    Task<PaymentIntent> CreateAsync(decimal amount, string currency, string? description);
    Task<PaymentIntent?> GetByIdAsync(string id);
    Task<IEnumerable<PaymentIntent>> GetAllAsync(PaymentIntentStatus? status = null);
    Task<PaymentIntent> ConfirmAsync(string id);
    Task<PaymentIntent> CaptureAsync(string id);
    Task<PaymentIntent> ReverseAsync(string id);
    Task<int> ExpirePendingAsync();
}
