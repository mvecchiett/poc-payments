using Microsoft.AspNetCore.Mvc;
using Payments.Application.Services;
using Payments.Shared.DTOs;
using Payments.Shared.Models;

namespace Payments.Api.Controllers;

[ApiController]
[Route("api/payment-intents")]
public class PaymentIntentsController : ControllerBase
{
    private readonly IPaymentIntentService _paymentIntentService;
    private readonly ILogger<PaymentIntentsController> _logger;

    public PaymentIntentsController(
        IPaymentIntentService paymentIntentService,
        ILogger<PaymentIntentsController> logger)
    {
        _paymentIntentService = paymentIntentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            _logger.LogInformation("Creating payment intent for amount: {Amount} {Currency}", 
                request.Amount, request.Currency);

            var intent = await _paymentIntentService.CreateAsync(
                request.Amount, 
                request.Currency, 
                request.Description);

            var response = MapToResponse(intent);
            return CreatedAtAction(nameof(GetPaymentIntent), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating payment intent");
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentIntent(string id)
    {
        _logger.LogInformation("Getting payment intent: {Id}", id);

        var intent = await _paymentIntentService.GetByIdAsync(id);
        if (intent == null)
        {
            return NotFound(new { error = $"Payment intent {id} not found" });
        }

        var response = MapToResponse(intent);
        return Ok(response);
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmPaymentIntent(string id)
    {
        try
        {
            _logger.LogInformation("Confirming payment intent: {Id}", id);

            var intent = await _paymentIntentService.ConfirmAsync(id);
            var response = MapToResponse(intent);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation confirming payment intent {Id}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/capture")]
    public async Task<IActionResult> CapturePaymentIntent(string id)
    {
        try
        {
            _logger.LogInformation("Capturing payment intent: {Id}", id);

            var intent = await _paymentIntentService.CaptureAsync(id);
            var response = MapToResponse(intent);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation capturing payment intent {Id}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/reverse")]
    public async Task<IActionResult> ReversePaymentIntent(string id)
    {
        try
        {
            _logger.LogInformation("Reversing payment intent: {Id}", id);

            var intent = await _paymentIntentService.ReverseAsync(id);
            var response = MapToResponse(intent);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation reversing payment intent {Id}", id);
            return Conflict(new { error = ex.Message });
        }
    }

    private static PaymentIntentResponse MapToResponse(PaymentIntent intent)
    {
        return new PaymentIntentResponse
        {
            Id = intent.Id,
            Status = intent.Status,
            Amount = intent.Amount,
            Currency = intent.Currency,
            Description = intent.Description,
            CreatedAt = intent.CreatedAt,
            UpdatedAt = intent.UpdatedAt,
            ConfirmedAt = intent.ConfirmedAt,
            ExpiresAt = intent.ExpiresAt,
            CapturedAt = intent.CapturedAt,
            ReversedAt = intent.ReversedAt,
            ExpiredAt = intent.ExpiredAt
        };
    }
}
