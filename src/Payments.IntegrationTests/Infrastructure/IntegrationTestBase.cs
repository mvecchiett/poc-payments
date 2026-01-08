using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Data;
using Xunit;

namespace Payments.IntegrationTests.Infrastructure;

/// <summary>
/// Clase base para todos los tests de integración
/// Provee HttpClient y helpers comunes
/// </summary>
public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly JsonSerializerOptions JsonOptions;

    public IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();

        // Configurar JSON options para coincidir con la API
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Obtiene el DbContext para operaciones directas (cuando sea necesario)
    /// </summary>
    protected PaymentsDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    }

    /// <summary>
    /// Helper para hacer requests POST con JSON
    /// </summary>
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content, JsonOptions);
    }

    /// <summary>
    /// Helper para parsear response JSON
    /// </summary>
    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public virtual void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
