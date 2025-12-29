@echo off
echo ============================================
echo Verificacion de Infraestructura Docker
echo ============================================
echo.

echo [1] Verificando estado de contenedores...
docker ps --filter "name=payments-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.

echo [2] Verificando salud de Postgres...
docker exec payments-postgres pg_isready -U postgres
if %ERRORLEVEL% EQU 0 (
    echo ✓ Postgres esta listo
) else (
    echo ✗ Postgres NO esta listo
)
echo.

echo [3] Verificando salud de Redis...
docker exec payments-redis redis-cli ping
if %ERRORLEVEL% EQU 0 (
    echo ✓ Redis esta listo
) else (
    echo ✗ Redis NO esta listo
)
echo.

echo [4] Verificando conectividad a Postgres (puerto 5432)...
powershell -Command "Test-NetConnection -ComputerName localhost -Port 5432 -InformationLevel Quiet"
if %ERRORLEVEL% EQU 0 (
    echo ✓ Puerto 5432 accesible
) else (
    echo ✗ Puerto 5432 NO accesible
)
echo.

echo [5] Verificando conectividad a Redis (puerto 6379)...
powershell -Command "Test-NetConnection -ComputerName localhost -Port 6379 -InformationLevel Quiet"
if %ERRORLEVEL% EQU 0 (
    echo ✓ Puerto 6379 accesible
) else (
    echo ✗ Puerto 6379 NO accesible
)
echo.

echo ============================================
echo Verificacion completada
echo ============================================
echo.
echo Si todos los checks son ✓, puedes ejecutar la API con:
echo   cd ..\src\Payments.Api
echo   dotnet run
echo.
pause
