@echo off
setlocal enabledelayedexpansion
echo ============================================
echo Diagnostico del Sistema
echo ============================================
echo.

cd /d "%~dp0"

echo [Información del entorno]
echo.
echo Directorio actual:
echo %CD%
echo.

echo Archivos .sln en este directorio:
dir /b *.sln 2>nul
if !ERRORLEVEL! NEQ 0 (
    echo   (No se encontraron archivos .sln)
)
echo.

echo Estructura de directorios:
dir /b /ad
echo.

echo [Verificación de proyectos]
echo.

if exist "src\Payments.Shared\Payments.Shared.csproj" (
    echo ✓ Payments.Shared encontrado
) else (
    echo ✗ Payments.Shared NO encontrado
)

if exist "src\Payments.Application\Payments.Application.csproj" (
    echo ✓ Payments.Application encontrado
) else (
    echo ✗ Payments.Application NO encontrado
)

if exist "src\Payments.Api\Payments.Api.csproj" (
    echo ✓ Payments.Api encontrado
) else (
    echo ✗ Payments.Api NO encontrado
)

if exist "src\Payments.Worker\Payments.Worker.csproj" (
    echo ✓ Payments.Worker encontrado
) else (
    echo ✗ Payments.Worker NO encontrado
)
echo.

echo [Verificación de carpetas críticas]
echo.

if exist "src\Payments.Application\Data" (
    echo ✓ Application\Data existe
    if exist "src\Payments.Application\Data\PaymentsDbContext.cs" (
        echo   ✓ PaymentsDbContext.cs existe
    ) else (
        echo   ✗ PaymentsDbContext.cs NO existe
    )
) else (
    echo ✗ Application\Data NO existe
)

if exist "src\Payments.Application\Services" (
    echo ✓ Application\Services existe
    if exist "src\Payments.Application\Services\PaymentIntentService.cs" (
        echo   ✓ PaymentIntentService.cs existe
    ) else (
        echo   ✗ PaymentIntentService.cs NO existe
    )
) else (
    echo ✗ Application\Services NO existe
)

if exist "src\Payments.Api\Data" (
    echo ⚠  Api\Data existe (debería estar eliminado)
) else (
    echo ✓ Api\Data NO existe (correcto)
)

if exist "src\Payments.Api\Services" (
    echo ⚠  Api\Services existe (debería estar eliminado)
) else (
    echo ✓ Api\Services NO existe (correcto)
)
echo.

echo [Verificación de Docker]
echo.

docker --version >nul 2>&1
if !ERRORLEVEL! EQU 0 (
    echo ✓ Docker instalado
    docker ps >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo ✓ Docker corriendo
        echo.
        echo Contenedores payments:
        docker ps --filter "name=payments" --format "table {{.Names}}\t{{.Status}}"
    ) else (
        echo ✗ Docker NO está corriendo
    )
) else (
    echo ✗ Docker NO está instalado
)
echo.

echo [Verificación de .NET]
echo.
dotnet --version
echo.

echo [Verificación de dotnet-ef]
echo.
dotnet ef --version >nul 2>&1
if !ERRORLEVEL! EQU 0 (
    echo ✓ dotnet-ef instalado
    dotnet ef --version
) else (
    echo ✗ dotnet-ef NO instalado
    echo   Instalar con: dotnet tool install --global dotnet-ef
)
echo.

echo ============================================
echo Diagnóstico completado
echo ============================================
echo.
pause
endlocal
