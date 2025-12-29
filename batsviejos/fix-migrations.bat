@echo off
setlocal enabledelayedexpansion
echo ============================================
echo FIX: Recrear Migraciones Correctamente
echo ============================================
echo.

cd /d "%~dp0src\Payments.Api"

echo [INFO] Este script va a:
echo   1. Eliminar migraciones existentes
echo   2. Eliminar __EFMigrationsHistory de Postgres
echo   3. Recrear la migracion inicial
echo   4. Aplicar la migracion (crear tabla payment_intents)
echo.
echo Presiona Ctrl+C para cancelar, o
pause

echo.
echo [1] Verificando Postgres...
docker ps --filter "name=payments-postgres" --format "{{.Status}}" 2>nul | findstr /C:"Up" >nul
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Postgres NO está corriendo
    pause
    exit /b 1
)
echo ✓ Postgres corriendo
echo.

echo [2] Eliminando carpeta Migrations...
if exist "Migrations" (
    rmdir /s /q "Migrations"
    echo ✓ Carpeta Migrations eliminada
) else (
    echo ✓ No hay carpeta Migrations
)
echo.

echo [3] Eliminando tabla __EFMigrationsHistory de Postgres...
docker exec payments-postgres psql -U postgres -d payments_db -c "DROP TABLE IF EXISTS \"__EFMigrationsHistory\" CASCADE;" >nul 2>&1
echo ✓ Tabla __EFMigrationsHistory eliminada
echo.

echo [4] Creando migración inicial...
dotnet ef migrations add InitialCreate
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Error creando migración
    pause
    exit /b 1
)
echo ✓ Migración creada
echo.

echo [5] Verificando archivos de migración...
if exist "Migrations\*_InitialCreate.cs" (
    echo ✓ Archivo de migración existe
) else (
    echo ✗ ERROR: No se creó el archivo de migración
    pause
    exit /b 1
)
echo.

echo [6] Aplicando migración a Postgres...
echo [INFO] Esto CREARÁ la tabla payment_intents
echo.
dotnet ef database update
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Error aplicando migración
    pause
    exit /b 1
)
echo ✓ Migración aplicada exitosamente
echo.

echo [7] Verificando tabla en Postgres...
docker exec payments-postgres psql -U postgres -d payments_db -c "\dt" | findstr /C:"payment_intents" >nul
if !ERRORLEVEL! EQU 0 (
    echo ✓ Tabla payment_intents existe!
) else (
    echo ✗ ERROR: Tabla payment_intents NO fue creada
    pause
    exit /b 1
)
echo.

echo ============================================
echo ¡Migraciones recreadas exitosamente!
echo ============================================
echo.
echo Verifica ejecutando:
echo   docker exec -it payments-postgres psql -U postgres -d payments_db
echo   \dt
echo.
echo Deberías ver:
echo   - __EFMigrationsHistory
echo   - payment_intents
echo.
pause
endlocal
