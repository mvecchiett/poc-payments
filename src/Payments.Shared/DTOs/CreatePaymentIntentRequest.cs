using System.ComponentModel.DataAnnotations;

namespace Payments.Shared.DTOs;

public class CreatePaymentIntentRequest
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be exactly 3 characters (ISO 4217 code)")]
    [RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "Currency must contain only letters")]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
