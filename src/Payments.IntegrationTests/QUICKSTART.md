# ⚡ Guía Rápida de Ejecución

## 🚀 Ejecutar tests en 3 pasos

### Paso 1: Agregar proyecto a la solución

```bash
cd "C:\DesarrolloC#\poc-payments"
dotnet sln poc-payments.sln add src/Payments.IntegrationTests/Payments.IntegrationTests.csproj
```

### Paso 2: Restaurar dependencias

```bash
dotnet restore
```

### Paso 3: Ejecutar tests

```bash
dotnet test src/Payments.IntegrationTests/Payments.IntegrationTests.csproj
```

## ✅ Resultado Esperado

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    26, Skipped:     0, Total:    26, Duration: 5 s
```

## 🎯 Tests por Categoría

```bash
# Solo validaciones (8 tests)
dotnet test --filter "FullyQualifiedName~ValidationTests"

# Solo workflow (9 tests)
dotnet test --filter "FullyQualifiedName~WorkflowTests"

# Solo GET endpoint (9 tests)
dotnet test --filter "FullyQualifiedName~GetEndpointTests"
```

## 📊 Ver Detalles

```bash
# Con output completo
dotnet test src/Payments.IntegrationTests/Payments.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

## 🐛 Troubleshooting

### Error: "Program type not found"

**Solución:** Verificar que `Program.cs` tenga al final:
```csharp
public partial class Program { }
```

### Error: "SqliteConnection was disposed"

**Solución:** El test está funcionando correctamente. SQLite in-memory se limpia automáticamente.

### Tests fallan por timeout

**Solución:** Aumentar timeout en los tests o verificar que no haya servicios bloqueantes.

## 🏆 Cobertura

- **26 tests** cubriendo:
  - ✅ Validaciones (shape + business)
  - ✅ Transiciones de estado
  - ✅ Error handling (400, 404, 409)
  - ✅ Filtrado y ordenamiento

## 📖 Documentación Completa

Ver `README.md` para detalles completos sobre:
- Arquitectura de tests
- Debugging
- Próximos pasos
