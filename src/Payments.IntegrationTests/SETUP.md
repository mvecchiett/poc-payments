# Instrucciones para Agregar el Proyecto de Tests

## Opción 1: Desde Visual Studio

1. Abrir la solución `C:\DesarrolloC#\poc-payments\poc-payments.sln`
2. Click derecho en la solución → "Add" → "Existing Project"
3. Navegar a `src\Payments.IntegrationTests\Payments.IntegrationTests.csproj`
4. Click "Open"
5. Build → Build Solution
6. Test → Run All Tests

## Opción 2: Desde línea de comandos

```bash
cd "C:\DesarrolloC#\poc-payments"

# Agregar proyecto a la solución
dotnet sln poc-payments.sln add src/Payments.IntegrationTests/Payments.IntegrationTests.csproj

# Restaurar dependencias
dotnet restore

# Build
dotnet build

# Ejecutar tests
dotnet test src/Payments.IntegrationTests/Payments.IntegrationTests.csproj
```

## Opción 3: Desde Rider

1. Abrir la solución
2. Click derecho en la solución → "Add" → "Existing Project"
3. Seleccionar `Payments.IntegrationTests.csproj`
4. Click derecho en el proyecto → "Run Unit Tests"

## Verificar que todo funciona

```bash
cd "C:\DesarrolloC#\poc-payments"

# Ejecutar tests con verbosity
dotnet test src/Payments.IntegrationTests/Payments.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

## Resultado esperado

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    26, Skipped:     0, Total:    26, Duration: 5 s
```

Si ves 26 tests pasados, ¡todo está funcionando correctamente! ✅
