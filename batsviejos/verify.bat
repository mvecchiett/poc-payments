@echo off
setlocal enabledelayedexpansion
echo ============================================
echo Verificacion Completa del Sistema
echo ============================================
echo.

cd /d "%~dp0"

SET ERRORS=0

REM Buscar archivo .sln
FOR %%F IN (*.sln) DO SET SOLUTION_FILE=%%F

if "!SOLUTION_FILE!"=="" (
    echo ✗ ERROR: No se encuentra archivo .sln
    SET /A ERRORS=!ERRORS!+1
) else (
    echo ✓ Solución encontrada: !SOLUTION_FILE!
)
echo.

echo [1] Verificando solución compila...
if not "!SOLUTION_FILE!"=="" (
    dotnet build "!SOLUTION_FILE!" --no-restore >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo ✓ Solución compila sin errores
    ) else (
        echo ✗ ERROR: La solución no compila
        SET /A ERRORS=!ERRORS!+1
    )
) else (
    echo ✗ SKIP: No hay solución para compilar
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo [2] Verificando Docker está corriendo...
docker ps >nul 2>&1
if !ERRORLEVEL! EQU 0 (
    echo ✓ Docker está corriendo
) else (
    echo ✗ ERROR: Docker NO está corriendo
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo [3] Verificando Postgres está levantado...
docker ps --filter "name=payments-postgres" --format "{{.Status}}" 2>nul | findstr /C:"Up" >nul
if !ERRORLEVEL! EQU 0 (
    echo ✓ Postgres está corriendo
) else (
    echo ✗ ERROR: Postgres NO está corriendo
    echo   Ejecuta: cd infra ^&^& docker compose up -d
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo [4] Verificando Redis está levantado...
docker ps --filter "name=payments-redis" --format "{{.Status}}" 2>nul | findstr /C:"Up" >nul
if !ERRORLEVEL! EQU 0 (
    echo ✓ Redis está corriendo
) else (
    echo ✗ ERROR: Redis NO está corriendo
    echo   Ejecuta: cd infra ^&^& docker compose up -d
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo [5] Verificando migraciones existen...
if exist "src\Payments.Api\Migrations" (
    echo ✓ Carpeta Migrations existe
) else (
    echo ⚠  Carpeta Migrations NO existe
    echo   Ejecuta: migrate.bat
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo [6] Verificando tabla en Postgres...
docker exec payments-postgres psql -U postgres -d payments_db -c "\dt payment_intents" >nul 2>&1
if !ERRORLEVEL! EQU 0 (
    echo ✓ Tabla payment_intents existe
) else (
    echo ✗ ERROR: Tabla payment_intents NO existe
    echo   Ejecuta: migrate.bat
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo [7] Verificando proyectos en solución...
if not "!SOLUTION_FILE!"=="" (
    dotnet sln "!SOLUTION_FILE!" list | findstr /C:"Payments.Application" >nul
    if !ERRORLEVEL! EQU 0 (
        echo ✓ Payments.Application en la solución
    ) else (
        echo ✗ ERROR: Payments.Application NO está en la solución
        SET /A ERRORS=!ERRORS!+1
    )
) else (
    echo ✗ SKIP: No hay solución
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo [8] Verificando Worker NO depende de API...
dotnet list src\Payments.Worker\Payments.Worker.csproj reference | findstr /C:"Payments.Api" >nul
if !ERRORLEVEL! NEQ 0 (
    echo ✓ Worker NO depende de API (correcto)
) else (
    echo ✗ ERROR: Worker depende de API (incorrecto)
    SET /A ERRORS=!ERRORS!+1
)
echo.

echo ============================================
echo Resumen
echo ============================================
echo.

if !ERRORS! EQU 0 (
    echo ✅ TODAS LAS VERIFICACIONES OK
    echo.
    echo Sistema listo para ejecutar:
    echo   1. run-api.bat
    echo   2. run-worker.bat
    echo   3. http://localhost:5000/swagger
    echo.
) else (
    echo ❌ SE ENCONTRARON !ERRORS! ERRORES
    echo.
    echo Revisa los pasos anteriores y corrige los errores.
    echo Consulta QUICK-START.md para instrucciones detalladas.
    echo.
)

pause
endlocal
