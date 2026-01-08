using System.Net;
using Payments.IntegrationTests.Helpers;
using Payments.IntegrationTests.Infrastructure;
using Payments.Shared.DTOs;
using Payments.Shared.Models;
using Xunit;

namespace Payments.IntegrationTests.Tests;

/// <summary>
/// Tests de validación de entrada (shape + business validation)
/// Verifican que el API retorne 400 Bad Request con mensajes apropiados
/// </summary>
public class ValidationTests : IntegrationTestBase
{
    public ValidationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreatePaymentIntent_WithValidCurrency_Returns201Created()
    {
        // Arrange
        var request = PaymentIntentFactory.CreateValid(
            amount: 10000m,
            currency: "ARS",
            description: "Valid payment"
        );

        // Act
        var response = await PostJsonAsync("/api/payment-intents", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await DeserializeResponseAsync<PaymentIntentResponse>(response);
        Assert.NotNull(result);
        Assert.Equal("ARS", result.Currency);
        Assert.Equal(10000m, result.Amount);
        Assert.Equal(PaymentIntentStatus.Created, result.Status);
    }

    [Fact]
    public async Task CreatePaymentIntent_WithLowercaseCurrency_Returns201AndNormalizesToUppercase()
    {
        // Arrange
        var request = PaymentIntentFactory.CreateWithLowercaseCurrency();

        // Act
        var response = await PostJsonAsync("/api/payment-intents", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await DeserializeResponseAsync<PaymentIntentResponse>(response);
        Assert.NotNull(result);
        Assert.Equal("ARS", result.Currency); // Debe estar en UPPERCASE
    }

    [Fact]
    public async Task CreatePaymentIntent_WithInvalidCurrencyLength_Returns400BadRequest()
    {
        // Arrange
        var request = PaymentIntentFactory.CreateWithInvalidCurrencyLength();

        // Act
        var response = await PostJsonAsync("/api/payment-intents", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Currency must be exactly 3 characters", content);
    }

    [Fact]
    public async Task CreatePaymentIntent_WithUnsupportedCurrency_Returns400BadRequest()
    {
        // Arrange
        var request = PaymentIntentFactory.CreateWithUnsupportedCurrency();

        // Act
        var response = await PostJsonAsync("/api/payment-intents", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid currency code", content);
        Assert.Contains("Valid currencies", content);
    }

    [Fact]
    public async Task CreatePaymentIntent_WithNegativeAmount_Returns400BadRequest()
    {
        // Arrange
        var request = PaymentIntentFactory.CreateWithInvalidAmount(-100m);

        // Act
        var response = await PostJsonAsync("/api/payment-intents", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Amount must be greater than zero", content);
    }

    [Fact]
    public async Task CreatePaymentIntent_WithZeroAmount_Returns400BadRequest()
    {
        // Arrange
        var request = PaymentIntentFactory.CreateWithInvalidAmount(0m);

        // Act
        var response = await PostJsonAsync("/api/payment-intents", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Amount must be greater than zero", content);
    }

    [Theory]
    [InlineData("ARS")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("BRL")]
    [InlineData("CLP")]
    public async Task CreatePaymentIntent_WithSupportedCurrencies_Returns201Created(string currency)
    {
        // Arrange
        var request = PaymentIntentFactory.CreateValid(currency: currency);

        // Act
        var response = await PostJsonAsync("/api/payment-intents", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await DeserializeResponseAsync<PaymentIntentResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(currency, result.Currency);
    }
}
