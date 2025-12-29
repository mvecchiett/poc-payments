namespace Payments.Shared.Models;

public class PaymentIntent
{
    public string Id { get; set; } = string.Empty;
    public PaymentIntentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
}
