@echo off
echo ============================================
echo Migraciones de Base de Datos
echo Este script CREA las tablas en Postgres
echo ============================================
echo.

REM Ir al proyecto API (donde están las migraciones)
cd /d "%~dp0src\Payments.Api"

echo [INFO] Directorio actual: %CD%
echo.

echo [1] Verificando conexión a Postgres...
docker ps --filter "name=payments-postgres" --format "{{.Status}}" | findstr /C:"Up" >nul
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Postgres NO está corriendo
    echo.
    echo Ejecuta primero:
    echo   cd infra
    echo   docker compose up -d
    echo.
    pause
    exit /b 1
)
echo ✓ Postgres está corriendo
echo.

echo [2] Verificando dotnet-ef...
dotnet ef --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ✗ dotnet-ef NO está instalado
    echo.
    echo Instalando dotnet-ef...
    dotnet tool install --global dotnet-ef
    if %ERRORLEVEL% NEQ 0 (
        echo ✗ Error instalando dotnet-ef
        pause
        exit /b 1
    )
    echo ✓ dotnet-ef instalado
)
echo ✓ dotnet-ef disponible
echo.

echo [3] Verificando si existen migraciones...
if not exist "Migrations" (
    echo No hay migraciones. Creando migración inicial...
    dotnet ef migrations add InitialCreateWithExpiresAt
    if %ERRORLEVEL% NEQ 0 (
        echo ✗ Error creando migración
        pause
        exit /b 1
    )
    echo ✓ Migración inicial creada
) else (
    echo ✓ Carpeta Migrations existe
)
echo.

echo [4] Aplicando migraciones a Postgres...
echo [INFO] Esto CREA las tablas: payment_intents, __EFMigrationsHistory
echo.
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error aplicando migraciones
    echo.
    echo Posibles causas:
    echo - Postgres no está accesible
    echo - Credenciales incorrectas
    echo - Base de datos no existe
    echo.
    pause
    exit /b 1
)
echo ✓ Base de datos actualizada exitosamente
echo.

echo ============================================
echo Tablas creadas en Postgres!
echo ============================================
echo.
echo La base de datos 'payments_db' contiene:
echo   - payment_intents (tabla principal)
echo   - __EFMigrationsHistory (control de versiones)
echo.
echo Puedes verificar con:
echo   docker exec -it payments-postgres psql -U postgres -d payments_db
echo   \dt
echo.
pause
