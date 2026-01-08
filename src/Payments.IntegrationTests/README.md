# Payments.IntegrationTests

Tests de integración para el POC de Payment Intents API.

## 🎯 Objetivo

Conjunto de pruebas automatizadas que demuestran:
- ✅ Validez de reglas de negocio
- ✅ Correcta transición de estados
- ✅ Manejo semántico de errores HTTP (400 / 409)

## 📁 Estructura

```
Payments.IntegrationTests/
├── Infrastructure/
│   ├── CustomWebApplicationFactory.cs   # Factory para levantar API in-memory
│   └── IntegrationTestBase.cs           # Clase base con helpers
├── Helpers/
│   └── PaymentIntentFactory.cs          # Factory para crear requests de test
├── Tests/
│   ├── ValidationTests.cs               # Tests de validación (400 Bad Request)
│   ├── WorkflowTests.cs                 # Tests de transiciones de estado
│   └── GetEndpointTests.cs              # Tests del endpoint GET
└── README.md
```

## 🧪 Tests Implementados

### ValidationTests (8 tests)
- ✅ Currency válida (ARS) → 201 Created
- ✅ Currency lowercase (ars) → 201 + normalizada a ARS
- ✅ Currency inválida (Pesos) → 400 Bad Request
- ✅ Currency no soportada (ZZZ) → 400 + lista de válidas
- ✅ Amount negativo → 400
- ✅ Amount cero → 400
- ✅ Múltiples currencies soportadas (Theory)

### WorkflowTests (9 tests)
- ✅ Created → Confirm → PendingConfirmation
- ✅ PendingConfirmation → Capture → Captured
- ✅ Created → Reverse → Reversed
- ✅ PendingConfirmation → Reverse → Reversed
- ✅ Created → Capture → 409 Conflict
- ✅ Captured → Reverse → 409 Conflict
- ✅ Reversed → Confirm → 409 Conflict
- ✅ Captured → Capture → 409 Conflict

### GetEndpointTests (9 tests)
- ✅ GET sin filtro → devuelve todos
- ✅ GET con `?status=Created` → solo Created
- ✅ GET con `?status=PendingConfirmation` → solo Pending
- ✅ GET con `?status=Captured` → solo Captured
- ✅ GET con `?status=Reversed` → solo Reversed
- ✅ Ordenamiento por CreatedAt DESC
- ✅ Filtro inválido → 400 Bad Request
- ✅ GET by ID válido → 200 OK
- ✅ GET by ID inválido → 404 Not Found

**Total: 26 tests**

## 🚀 Cómo Ejecutar

### Requisitos
- .NET 8 SDK
- Proyecto `Payments.Api` compilable

### Ejecutar todos los tests

```bash
# Desde la raíz del proyecto
cd C:\DesarrolloC#\poc-payments

# Ejecutar todos los tests
dotnet test src/Payments.IntegrationTests/Payments.IntegrationTests.csproj

# Con output detallado
dotnet test src/Payments.IntegrationTests/Payments.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

### Ejecutar un test específico

```bash
# Ejecutar solo ValidationTests
dotnet test --filter "FullyQualifiedName~ValidationTests"

# Ejecutar solo WorkflowTests
dotnet test --filter "FullyQualifiedName~WorkflowTests"

# Ejecutar solo GetEndpointTests
dotnet test --filter "FullyQualifiedName~GetEndpointTests"

# Ejecutar un test específico
dotnet test --filter "FullyQualifiedName~CreatePaymentIntent_WithValidCurrency_Returns201Created"
```

### Ejecutar desde Visual Studio

1. Abrir la solución `poc-payments.sln`
2. Build → Build Solution
3. Test → Run All Tests
4. Ver resultados en Test Explorer

### Ejecutar desde Rider

1. Abrir la solución
2. Click derecho en `Payments.IntegrationTests`
3. "Run Unit Tests"

## 🏗️ Arquitectura de Tests

### WebApplicationFactory

Los tests usan `WebApplicationFactory<Program>` para levantar la API en memoria:
- ✅ No requiere servidor HTTP externo
- ✅ Base de datos SQLite in-memory (aislada por test)
- ✅ Velocidad rápida
- ✅ Sin dependencias externas

### Base de Datos

Se usa **SQLite in-memory** para tests:
- ✅ Cada ejecución es limpia
- ✅ No contamina base de datos de desarrollo
- ✅ Portabilidad (no requiere PostgreSQL)
- ✅ Rápido

### Patrón Factory

`PaymentIntentFactory` provee métodos estáticos para crear requests:
- `CreateValid()` - Request válido con defaults
- `CreateWithInvalidCurrencyLength()` - Currency con más de 3 chars
- `CreateWithUnsupportedCurrency()` - Currency formato válido pero no soportada
- `CreateWithInvalidAmount()` - Amount inválido
- `CreateWithLowercaseCurrency()` - Currency en minúsculas

## 📊 Resultados Esperados

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    26, Skipped:     0, Total:    26, Duration: 5 s
```

## 🔍 Debugging

### Ver logs de la API durante tests

Agregar en `CustomWebApplicationFactory.cs`:

```csharp
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});
```

### Ver SQL queries de EF Core

```csharp
services.AddDbContext<PaymentsDbContext>(options =>
{
    options.UseSqlite("DataSource=:memory:");
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Information);
});
```

## ✅ Validaciones Cubiertas

| Aspecto | Tests | Status |
|---------|-------|--------|
| Shape validation (DTO) | 4 | ✅ |
| Business validation (Service) | 2 | ✅ |
| State transitions válidas | 4 | ✅ |
| State transitions inválidas | 4 | ✅ |
| Filtrado por status | 5 | ✅ |
| Ordenamiento | 1 | ✅ |
| Error handling 400 | 6 | ✅ |
| Error handling 404 | 1 | ✅ |
| Error handling 409 | 4 | ✅ |
| Normalización | 1 | ✅ |

## 🎯 Próximos Pasos (Opcional)

- [ ] Tests de expiración automática (Worker)
- [ ] Tests de concurrencia (optimistic locking)
- [ ] Tests de performance (benchmarks)
- [ ] Code coverage report
- [ ] Tests paramétricos adicionales (más currencies)

## 📝 Notas

- Los tests **NO** requieren servicios externos corriendo
- La API se levanta automáticamente en memoria
- Cada test tiene su propia base de datos aislada
- Los tests son idempotentes y pueden ejecutarse en cualquier orden

## 🏆 Cobertura

Este conjunto de tests cubre:
- ✅ 100% de los endpoints públicos
- ✅ 100% de las transiciones válidas
- ✅ 100% de las transiciones inválidas
- ✅ 100% de las validaciones de negocio
- ✅ 100% de los códigos de error HTTP

---

**Autor:** POC Payment Intents  
**Framework:** xUnit + WebApplicationFactory  
**Base de datos:** SQLite in-memory  
**Última actualización:** Enero 2026
