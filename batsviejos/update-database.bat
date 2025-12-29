@echo off
echo ============================================
echo Actualizando Base de Datos (Migracion ExpiresAt)
echo ============================================
echo.

cd /d "%~dp0src\Payments.Api"

echo [1] Creando nueva migracion...
dotnet ef migrations add AddExpiresAtField
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error creando migracion
    pause
    exit /b 1
)
echo ✓ Migracion creada
echo.

echo [2] Aplicando migracion a la base de datos...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error aplicando migracion
    pause
    exit /b 1
)
echo ✓ Base de datos actualizada
echo.

echo ============================================
echo Migracion aplicada exitosamente!
echo ============================================
echo.
echo El campo expires_at ha sido agregado a la tabla payment_intents.
echo Ahora puedes ejecutar:
echo   - API: cd src\Payments.Api ^&^& dotnet run
echo   - Worker: cd src\Payments.Worker ^&^& dotnet run
echo.
pause
