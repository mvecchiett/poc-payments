using System.Net;
using Payments.IntegrationTests.Helpers;
using Payments.IntegrationTests.Infrastructure;
using Payments.Shared.DTOs;
using Payments.Shared.Models;
using Xunit;

namespace Payments.IntegrationTests.Tests;

/// <summary>
/// Tests de workflow y transiciones de estado
/// Verifican que las reglas de negocio se cumplan correctamente
/// </summary>
public class WorkflowTests : IntegrationTestBase
{
    public WorkflowTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Workflow_Created_Confirm_ReturnsPendingConfirmation()
    {
        // Arrange - Crear intent
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        // Act - Confirmar
        var confirmResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/confirm", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        
        var confirmedIntent = await DeserializeResponseAsync<PaymentIntentResponse>(confirmResponse);
        Assert.NotNull(confirmedIntent);
        Assert.Equal(PaymentIntentStatus.PendingConfirmation, confirmedIntent.Status);
        Assert.NotNull(confirmedIntent.ConfirmedAt);
        Assert.NotNull(confirmedIntent.ExpiresAt);
    }

    [Fact]
    public async Task Workflow_PendingConfirmation_Capture_ReturnsCaptured()
    {
        // Arrange - Crear y confirmar intent
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        await Client.PostAsync($"/api/payment-intents/{intent.Id}/confirm", null);

        // Act - Capturar
        var captureResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/capture", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, captureResponse.StatusCode);
        
        var capturedIntent = await DeserializeResponseAsync<PaymentIntentResponse>(captureResponse);
        Assert.NotNull(capturedIntent);
        Assert.Equal(PaymentIntentStatus.Captured, capturedIntent.Status);
        Assert.NotNull(capturedIntent.CapturedAt);
        Assert.Null(capturedIntent.ExpiresAt); // ExpiresAt debe limpiarse
    }

    [Fact]
    public async Task Workflow_Created_Reverse_ReturnsReversed()
    {
        // Arrange - Crear intent
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        // Act - Revertir desde Created
        var reverseResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/reverse", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, reverseResponse.StatusCode);
        
        var reversedIntent = await DeserializeResponseAsync<PaymentIntentResponse>(reverseResponse);
        Assert.NotNull(reversedIntent);
        Assert.Equal(PaymentIntentStatus.Reversed, reversedIntent.Status);
        Assert.NotNull(reversedIntent.ReversedAt);
    }

    [Fact]
    public async Task Workflow_PendingConfirmation_Reverse_ReturnsReversed()
    {
        // Arrange - Crear y confirmar intent
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        await Client.PostAsync($"/api/payment-intents/{intent.Id}/confirm", null);

        // Act - Revertir desde PendingConfirmation
        var reverseResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/reverse", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, reverseResponse.StatusCode);
        
        var reversedIntent = await DeserializeResponseAsync<PaymentIntentResponse>(reverseResponse);
        Assert.NotNull(reversedIntent);
        Assert.Equal(PaymentIntentStatus.Reversed, reversedIntent.Status);
        Assert.NotNull(reversedIntent.ReversedAt);
    }

    [Fact]
    public async Task Workflow_Created_Capture_Returns409Conflict()
    {
        // Arrange - Crear intent en estado Created
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        // Act - Intentar capturar sin confirmar primero
        var captureResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/capture", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, captureResponse.StatusCode);
        
        var content = await captureResponse.Content.ReadAsStringAsync();
        Assert.Contains("Cannot capture payment intent in status Created", content);
    }

    [Fact]
    public async Task Workflow_Captured_Reverse_Returns409Conflict()
    {
        // Arrange - Crear, confirmar y capturar intent
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        await Client.PostAsync($"/api/payment-intents/{intent.Id}/confirm", null);
        await Client.PostAsync($"/api/payment-intents/{intent.Id}/capture", null);

        // Act - Intentar revertir un intent ya capturado
        var reverseResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/reverse", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, reverseResponse.StatusCode);
        
        var content = await reverseResponse.Content.ReadAsStringAsync();
        Assert.Contains("Cannot reverse payment intent in status Captured", content);
    }

    [Fact]
    public async Task Workflow_Reversed_Confirm_Returns409Conflict()
    {
        // Arrange - Crear y revertir intent
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        await Client.PostAsync($"/api/payment-intents/{intent.Id}/reverse", null);

        // Act - Intentar confirmar un intent ya revertido
        var confirmResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/confirm", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
        
        var content = await confirmResponse.Content.ReadAsStringAsync();
        Assert.Contains("Cannot confirm payment intent in status Reversed", content);
    }

    [Fact]
    public async Task Workflow_Captured_Capture_Returns409Conflict()
    {
        // Arrange - Crear, confirmar y capturar intent
        var createRequest = PaymentIntentFactory.CreateValid();
        var createResponse = await PostJsonAsync("/api/payment-intents", createRequest);
        var intent = await DeserializeResponseAsync<PaymentIntentResponse>(createResponse);
        Assert.NotNull(intent);

        await Client.PostAsync($"/api/payment-intents/{intent.Id}/confirm", null);
        await Client.PostAsync($"/api/payment-intents/{intent.Id}/capture", null);

        // Act - Intentar capturar nuevamente
        var captureResponse = await Client.PostAsync($"/api/payment-intents/{intent.Id}/capture", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, captureResponse.StatusCode);
        
        var content = await captureResponse.Content.ReadAsStringAsync();
        Assert.Contains("Cannot capture payment intent in status Captured", content);
    }
}
