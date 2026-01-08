using System.Net;
using Payments.IntegrationTests.Helpers;
using Payments.IntegrationTests.Infrastructure;
using Payments.Shared.DTOs;
using Payments.Shared.Models;
using Xunit;

namespace Payments.IntegrationTests.Tests;

/// <summary>
/// Tests del endpoint GET /api/payment-intents
/// Verifican filtrado, ordenamiento y respuesta correcta
/// </summary>
public class GetEndpointTests : IntegrationTestBase
{
    public GetEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllPaymentIntents_WithoutFilter_ReturnsAllIntents()
    {
        // Arrange - Crear 3 intents en diferentes estados
        var request1 = PaymentIntentFactory.CreateValid(description: "Intent 1");
        var request2 = PaymentIntentFactory.CreateValid(description: "Intent 2");
        var request3 = PaymentIntentFactory.CreateValid(description: "Intent 3");

        var response1 = await PostJsonAsync("/api/payment-intents", request1);
        var intent1 = await DeserializeResponseAsync<PaymentIntentResponse>(response1);
        
        var response2 = await PostJsonAsync("/api/payment-intents", request2);
        var intent2 = await DeserializeResponseAsync<PaymentIntentResponse>(response2);
        
        var response3 = await PostJsonAsync("/api/payment-intents", request3);
        var intent3 = await DeserializeResponseAsync<PaymentIntentResponse>(response3);

        // Confirmar el segundo
        await Client.PostAsync($"/api/payment-intents/{intent2!.Id}/confirm", null);

        // Act - Obtener todos
        var getResponse = await Client.GetAsync("/api/payment-intents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var intents = await DeserializeResponseAsync<List<PaymentIntentResponse>>(getResponse);
        Assert.NotNull(intents);
        Assert.True(intents.Count >= 3, $"Expected at least 3 intents, got {intents.Count}");
    }

    [Fact]
    public async Task GetAllPaymentIntents_WithStatusFilter_ReturnsOnlyMatchingIntents()
    {
        // Arrange - Crear intents en diferentes estados
        var createRequest = PaymentIntentFactory.CreateValid(description: "Created Intent");
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var createdIntent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);

        var confirmRequest = PaymentIntentFactory.CreateValid(description: "Pending Intent");
        var confirmResponse = await PostJsonAsync("/api/payment-intents", confirmRequest);
        var pendingIntent = await DeserializeResponseAsync<PaymentIntentResponse>(confirmResponse);
        await Client.PostAsync($"/api/payment-intents/{pendingIntent!.Id}/confirm", null);

        // Act - Filtrar solo Created
        var getResponse = await Client.GetAsync("/api/payment-intents?status=Created");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var intents = await DeserializeResponseAsync<List<PaymentIntentResponse>>(getResponse);
        Assert.NotNull(intents);
        Assert.All(intents, intent => Assert.Equal(PaymentIntentStatus.Created, intent.Status));
        Assert.Contains(intents, i => i.Id == createdIntent!.Id);
    }

    [Fact]
    public async Task GetAllPaymentIntents_WithPendingConfirmationFilter_ReturnsOnlyPending()
    {
        // Arrange - Crear y confirmar un intent
        var request = PaymentIntentFactory.CreateValid(description: "Pending Intent");
        var createResponse = await PostJsonAsync("/api/payment-intents", request);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        await Client.PostAsync($"/api/payment-intents/{intent!.Id}/confirm", null);

        // Act - Filtrar solo PendingConfirmation
        var getResponse = await Client.GetAsync("/api/payment-intents?status=PendingConfirmation");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var intents = await DeserializeResponseAsync<List<PaymentIntentResponse>>(getResponse);
        Assert.NotNull(intents);
        Assert.All(intents, i => Assert.Equal(PaymentIntentStatus.PendingConfirmation, i.Status));
        Assert.Contains(intents, i => i.Id == intent.Id);
    }

    [Fact]
    public async Task GetAllPaymentIntents_OrderedByCreatedAtDescending()
    {
        // Arrange - Crear 3 intents con delays para asegurar orden
        var request1 = PaymentIntentFactory.CreateValid(description: "First");
        await PostJsonAsync("/api/payment-intents", request1);
        await Task.Delay(100);

        var request2 = PaymentIntentFactory.CreateValid(description: "Second");
        await PostJsonAsync("/api/payment-intents", request2);
        await Task.Delay(100);

        var request3 = PaymentIntentFactory.CreateValid(description: "Third");
        var response3 = await PostJsonAsync("/api/payment-intents", request3);
        var lastIntent = await DeserializeResponseAsync<PaymentIntentResponse>(response3);

        // Act
        var getResponse = await Client.GetAsync("/api/payment-intents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var intents = await DeserializeResponseAsync<List<PaymentIntentResponse>>(getResponse);
        Assert.NotNull(intents);
        Assert.True(intents.Count >= 3);

        // El más reciente debe estar primero
        Assert.Equal(lastIntent!.Id, intents[0].Id);

        // Verificar orden descendente
        for (int i = 1; i < intents.Count; i++)
        {
            Assert.True(
                intents[i - 1].CreatedAt >= intents[i].CreatedAt,
                $"Expected descending order: {intents[i - 1].CreatedAt} >= {intents[i].CreatedAt}"
            );
        }
    }

    [Fact]
    public async Task GetAllPaymentIntents_WithInvalidStatusFilter_Returns400BadRequest()
    {
        // Act - Usar un status inválido
        var getResponse = await Client.GetAsync("/api/payment-intents?status=InvalidStatus");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        
        var content = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("Invalid status value", content);
    }

    [Fact]
    public async Task GetAllPaymentIntents_WithCapturedFilter_ReturnsOnlyCaptured()
    {
        // Arrange - Crear, confirmar y capturar
        var request = PaymentIntentFactory.CreateValid(description: "Captured Intent");
        var createResponse = await PostJsonAsync("/api/payment-intents", request);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        
        await Client.PostAsync($"/api/payment-intents/{intent!.Id}/confirm", null);
        await Client.PostAsync($"/api/payment-intents/{intent.Id}/capture", null);

        // Act
        var getResponse = await Client.GetAsync("/api/payment-intents?status=Captured");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var intents = await DeserializeResponseAsync<List<PaymentIntentResponse>>(getResponse);
        Assert.NotNull(intents);
        Assert.All(intents, i => Assert.Equal(PaymentIntentStatus.Captured, i.Status));
        Assert.Contains(intents, i => i.Id == intent.Id);
    }

    [Fact]
    public async Task GetAllPaymentIntents_WithReversedFilter_ReturnsOnlyReversed()
    {
        // Arrange - Crear y revertir
        var request = PaymentIntentFactory.CreateValid(description: "Reversed Intent");
        var createResponse = await PostJsonAsync("/api/payment-intents", request);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        
        await Client.PostAsync($"/api/payment-intents/{intent!.Id}/reverse", null);

        // Act
        var getResponse = await Client.GetAsync("/api/payment-intents?status=Reversed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var intents = await DeserializeResponseAsync<List<PaymentIntentResponse>>(getResponse);
        Assert.NotNull(intents);
        Assert.All(intents, i => Assert.Equal(PaymentIntentStatus.Reversed, i.Status));
        Assert.Contains(intents, i => i.Id == intent.Id);
    }

    [Fact]
    public async Task GetPaymentIntentById_WithValidId_ReturnsIntent()
    {
        // Arrange
        var request = PaymentIntentFactory.CreateValid(description: "Get By Id Test");
        var createResponse = await PostJsonAsync("/api/payment-intents", request);
        var createdIntent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);

        // Act
        var getResponse = await Client.GetAsync($"/api/payment-intents/{createdIntent!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(getResponse);
        Assert.NotNull(intent);
        Assert.Equal(createdIntent.Id, intent.Id);
        Assert.Equal(createdIntent.Amount, intent.Amount);
        Assert.Equal(createdIntent.Currency, intent.Currency);
    }

    [Fact]
    public async Task GetPaymentIntentById_WithInvalidId_Returns404NotFound()
    {
        // Act
        var getResponse = await Client.GetAsync("/api/payment-intents/pi_nonexistent123");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
