using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Data.Sqlite;
using Payments.Application.Data;

namespace Payments.IntegrationTests.Infrastructure;

/// <summary>
/// Factory para crear una instancia de la API con SQLite in-memory para tests.
/// Mantiene una conexión única abierta para evitar pérdida de datos entre requests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover el DbContext existente de PostgreSQL
            services.RemoveAll(typeof(DbContextOptions<PaymentsDbContext>));

            // Crear y mantener una conexión SQLite in-memory única
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Registrar DbContext con la conexión persistente
            services.AddDbContext<PaymentsDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging();
            });

            // Crear la base de datos dentro de un scope
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<PaymentsDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
