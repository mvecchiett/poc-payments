using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payments.Application.Data;
using Payments.Shared.Models;

namespace Payments.Application.Services;

public class PaymentIntentService : IPaymentIntentService
{
    private readonly PaymentsDbContext _context;
    private readonly ILogger<PaymentIntentService> _logger;
    private readonly IConfiguration _configuration;

    public PaymentIntentService(
        PaymentsDbContext context, 
        ILogger<PaymentIntentService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<PaymentIntent> CreateAsync(decimal amount, string currency, string? description)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero");
        }

        var intent = new PaymentIntent
        {
            Id = $"pi_{Guid.NewGuid():N}",
            Status = PaymentIntentStatus.Created,
            Amount = amount,
            Currency = currency,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PaymentIntents.Add(intent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment intent created: {Id}", intent.Id);
        return intent;
    }

    public async Task<PaymentIntent?> GetByIdAsync(string id)
    {
        return await _context.PaymentIntents
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PaymentIntent>> GetAllAsync(PaymentIntentStatus? status = null)
    {
        var query = _context.PaymentIntents.AsQueryable();

        // Filtrar por status si se especifica
        if (status.HasValue)
        {            query = query.Where(p => p.Status == status.Value);
        }

        // Ordenar por fecha de creación descendente (más recientes primero)
        var intents = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} payment intents (status filter: {Status})", 
            intents.Count, status?.ToString() ?? "all");

        return intents;
    }

    public async Task<PaymentIntent> ConfirmAsync(string id)
    {
        var intent = await GetByIdAsync(id);
        if (intent == null)
        {
            throw new InvalidOperationException($"Payment intent {id} not found");
        }

        // Validar transición: solo desde Created
        if (intent.Status != PaymentIntentStatus.Created)
        {
            throw new InvalidOperationException(
                $"Cannot confirm payment intent in status {intent.Status}. Must be in Created status.");
        }

        var expirationTimeoutSeconds = _configuration.GetValue<int>("PaymentSettings:ExpirationTimeoutSeconds", 120);
        var now = DateTime.UtcNow;

        intent.Status = PaymentIntentStatus.PendingConfirmation;
        intent.ConfirmedAt = now;
        intent.UpdatedAt = now;
        intent.ExpiresAt = now.AddSeconds(expirationTimeoutSeconds);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment intent confirmed: {Id}, expires at: {ExpiresAt}", 
            intent.Id, intent.ExpiresAt);
        return intent;
    }

    public async Task<PaymentIntent> CaptureAsync(string id)
    {
        var intent = await GetByIdAsync(id);
        if (intent == null)
        {
            throw new InvalidOperationException($"Payment intent {id} not found");
        }

        // Validar transición: solo desde PendingConfirmation
        if (intent.Status != PaymentIntentStatus.PendingConfirmation)
        {
            throw new InvalidOperationException(
                $"Cannot capture payment intent in status {intent.Status}. Must be in PendingConfirmation status.");
        }

        intent.Status = PaymentIntentStatus.Captured;
        intent.CapturedAt = DateTime.UtcNow;
        intent.UpdatedAt = DateTime.UtcNow;
        intent.ExpiresAt = null; // Limpiar: ya no puede expirar

        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment intent captured: {Id}", intent.Id);
        return intent;
    }

    public async Task<PaymentIntent> ReverseAsync(string id)
    {
        var intent = await GetByIdAsync(id);
        if (intent == null)
        {
            throw new InvalidOperationException($"Payment intent {id} not found");
        }

        // Validar transición: solo desde Created o PendingConfirmation
        if (intent.Status != PaymentIntentStatus.Created && 
            intent.Status != PaymentIntentStatus.PendingConfirmation)
        {
            throw new InvalidOperationException(
                $"Cannot reverse payment intent in status {intent.Status}. Must be in Created or PendingConfirmation status.");
        }

        intent.Status = PaymentIntentStatus.Reversed;
        intent.ReversedAt = DateTime.UtcNow;
        intent.UpdatedAt = DateTime.UtcNow;
        intent.ExpiresAt = null; // Limpiar: ya no puede expirar

        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment intent reversed: {Id}", intent.Id);
        return intent;
    }

    public async Task<int> ExpirePendingAsync()
    {
        var now = DateTime.UtcNow;

        // Buscar intents en PendingConfirmation que ya expiraron
        var expiredIntents = await _context.PaymentIntents
            .Where(p => 
                p.Status == PaymentIntentStatus.PendingConfirmation &&
                p.ExpiresAt.HasValue &&
                p.ExpiresAt.Value <= now)
            .ToListAsync();

        if (!expiredIntents.Any())
        {
            return 0;
        }

        foreach (var intent in expiredIntents)
        {
            intent.Status = PaymentIntentStatus.Expired;
            intent.ExpiredAt = now;
            intent.UpdatedAt = now;
            intent.ExpiresAt = null; // Limpiar: ya expiró

            _logger.LogInformation("Payment intent expired: {Id}, was pending since {ConfirmedAt}", 
                intent.Id, intent.ConfirmedAt);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Expired {Count} payment intents", expiredIntents.Count);
        return expiredIntents.Count;
    }
}
