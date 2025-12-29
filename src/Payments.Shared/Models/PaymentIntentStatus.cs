namespace Payments.Shared.Models;

public enum PaymentIntentStatus
{
    Created,
    PendingConfirmation,
    Captured,
    Reversed,
    Expired
}
