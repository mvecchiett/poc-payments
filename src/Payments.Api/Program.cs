using Microsoft.EntityFrameworkCore;
using Payments.Application.Data;
using Payments.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS para permitir requests desde el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configurar DbContext con Postgres
// IMPORTANTE: MigrationsAssembly apunta a Payments.Api donde están las migraciones
builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresConnection"),
        b => b.MigrationsAssembly("Payments.Api")));

// Registrar servicios
builder.Services.AddScoped<IPaymentIntentService, PaymentIntentService>();

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Habilitar CORS
app.UseCors("AllowFrontend");

// Swagger habilitado en todos los ambientes para el POC
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Intent API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

app.Run();

// Hacer Program accesible para tests de integración
public partial class Program { }
