using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payments.Application.Data;
using Payments.Application.Services;
using Microsoft.Extensions.Logging;
using Payments.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configurar DbContext con Postgres
// IMPORTANTE: MigrationsAssembly apunta a Payments.Api donde están las migraciones
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresConnection"),
        b => b.MigrationsAssembly("Payments.Api")));

// Registrar servicios
builder.Services.AddScoped<IPaymentIntentService, PaymentIntentService>();

// Registrar el worker
builder.Services.AddHostedService<ExpirationWorkerService>();

// Configurar logging
builder.Logging.AddConsole();

var host = builder.Build();
host.Run();
