using Payments.Shared.DTOs;

namespace Payments.IntegrationTests.Helpers;

/// <summary>
/// Factory para crear PaymentIntents de prueba con valores por defecto
/// </summary>
public static class PaymentIntentFactory
{
    /// <summary>
    /// Crea un request válido con valores por defecto
    /// </summary>
    public static CreatePaymentIntentRequest CreateValid(
        decimal? amount = null,
        string? currency = null,
        string? description = null)
    {
        return new CreatePaymentIntentRequest
        {
            Amount = amount ?? 10000m,
            Currency = currency ?? "ARS",
            Description = description ?? "Test payment intent"
        };
    }

    /// <summary>
    /// Crea un request con currency inválida (más de 3 caracteres)
    /// </summary>
    public static CreatePaymentIntentRequest CreateWithInvalidCurrencyLength()
    {
        return new CreatePaymentIntentRequest
        {
            Amount = 10000m,
            Currency = "Pesos", // 5 caracteres
            Description = "Invalid currency length"
        };
    }

    /// <summary>
    /// Crea un request con currency no soportada (pero formato válido)
    /// </summary>
    public static CreatePaymentIntentRequest CreateWithUnsupportedCurrency()
    {
        return new CreatePaymentIntentRequest
        {
            Amount = 10000m,
            Currency = "ZZZ", // 3 letras pero no en la lista
            Description = "Unsupported currency"
        };
    }

    /// <summary>
    /// Crea un request con amount inválido
    /// </summary>
    public static CreatePaymentIntentRequest CreateWithInvalidAmount(decimal amount)
    {
        return new CreatePaymentIntentRequest
        {
            Amount = amount,
            Currency = "ARS",
            Description = "Invalid amount"
        };
    }

    /// <summary>
    /// Crea un request con currency lowercase
    /// </summary>
    public static CreatePaymentIntentRequest CreateWithLowercaseCurrency()
    {
        return new CreatePaymentIntentRequest
        {
            Amount = 10000m,
            Currency = "ars", // lowercase
            Description = "Lowercase currency"
        };
    }
}
