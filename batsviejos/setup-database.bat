@echo off
echo ============================================
echo Creando Migraciones de Base de Datos
echo ============================================
echo.

cd /d "%~dp0src\Payments.Api"

echo [1] Restaurando paquetes NuGet...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error restaurando paquetes
    pause
    exit /b 1
)
echo ✓ Paquetes restaurados
echo.

echo [2] Creando migracion inicial...
dotnet ef migrations add InitialCreate
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error creando migracion
    echo.
    echo Asegurate de tener instalado dotnet-ef:
    echo   dotnet tool install --global dotnet-ef
    pause
    exit /b 1
)
echo ✓ Migracion creada
echo.

echo [3] Aplicando migracion a la base de datos...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error aplicando migracion
    echo Verifica que Docker este corriendo y Postgres este accesible
    pause
    exit /b 1
)
echo ✓ Base de datos actualizada
echo.

echo ============================================
echo Migraciones aplicadas exitosamente!
echo ============================================
echo.
echo La tabla payment_intents ha sido creada en Postgres.
echo Ahora puedes ejecutar la API con: dotnet run
echo.
pause
