@echo off
echo ============================================
echo Fase 1B - Migracion Final (ExpiresAt)
echo ============================================
echo.

cd /d "%~dp0src\Payments.Api"

echo [1] Limpiando migraciones anteriores...
dotnet ef database drop --force
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error al eliminar base de datos
    echo Esto puede ocurrir si la DB no existe todavia, continuando...
)
echo ✓ Base de datos limpia
echo.

echo [2] Eliminando carpeta de migraciones...
if exist "Migrations" (
    rmdir /s /q "Migrations"
    echo ✓ Carpeta Migrations eliminada
) else (
    echo ✓ No hay carpeta Migrations
)
echo.

echo [3] Creando migracion inicial completa...
dotnet ef migrations add InitialCreateWithExpiresAt
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error creando migracion
    pause
    exit /b 1
)
echo ✓ Migracion creada
echo.

echo [4] Aplicando migracion a Postgres...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error aplicando migracion
    pause
    exit /b 1
)
echo ✓ Base de datos creada exitosamente
echo.

echo ============================================
echo Migracion Fase 1B completada!
echo ============================================
echo.
echo La tabla payment_intents incluye:
echo   - Todos los campos de estado
echo   - Campo expires_at con indice
echo   - Indices optimizados para consultas
echo.
echo Arquitectura actualizada:
echo   API       → Payments.Application
echo   Worker    → Payments.Application
echo   (Desacoplados correctamente)
echo.
pause
