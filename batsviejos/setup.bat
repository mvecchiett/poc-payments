@echo off
setlocal enabledelayedexpansion
echo ============================================
echo Setup Completo - Fase 1B Safe Refactor
echo ============================================
echo.

REM Asegurarse de estar en la raíz del proyecto
cd /d "%~dp0"

echo [INFO] Directorio actual: %CD%
echo.

echo [Paso 1] Limpiando duplicados en API...
call cleanup-duplicates.bat
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Error en limpieza
    pause
    exit /b 1
)
echo.

echo [Paso 2] Verificando solución...
dir /b *.sln >nul 2>&1
if !ERRORLEVEL! NEQ 0 (
    echo ✗ ERROR: No se encuentra ningún archivo .sln
    echo ✗ Directorio actual: %CD%
    pause
    exit /b 1
)

FOR %%F IN (*.sln) DO SET SOLUTION_FILE=%%F

echo ✓ Solución encontrada: !SOLUTION_FILE!
echo.

echo [Paso 3] Agregando Payments.Application a la solución...
dotnet sln "!SOLUTION_FILE!" list 2>nul | findstr /C:"Payments.Application" >nul
if !ERRORLEVEL! NEQ 0 (
    echo Agregando proyecto...
    dotnet sln "!SOLUTION_FILE!" add src\Payments.Application\Payments.Application.csproj
    echo ✓ Payments.Application agregado
) else (
    echo ✓ Payments.Application ya existe en la solución
)
echo.

echo [Paso 4] Restaurando paquetes NuGet...
dotnet restore "!SOLUTION_FILE!"
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Error restaurando paquetes
    pause
    exit /b 1
)
echo ✓ Paquetes restaurados
echo.

echo [Paso 5] Compilando solución...
dotnet build "!SOLUTION_FILE!" --no-restore
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Error compilando
    pause
    exit /b 1
)
echo ✓ Solución compilada exitosamente
echo.

echo [Paso 6] Verificando infraestructura Docker...
docker ps >nul 2>&1
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Docker NO está corriendo
    echo   Inicia Docker Desktop y vuelve a ejecutar este script
    pause
    exit /b 1
)

REM Verificación rápida de Postgres
docker ps 2>nul | findstr /C:"payments-postgres" | findstr /C:"Up" >nul
if !ERRORLEVEL! NEQ 0 (
    echo.
    echo ⚠️  ADVERTENCIA: Postgres NO está corriendo
    echo.
    echo Para levantar la infraestructura:
    echo   cd infra
    echo   docker compose up -d
    echo   cd ..
    echo.
    echo ¿Deseas continuar sin migrar la base de datos? (S/N)
    set /p CONTINUAR=
    if /i not "!CONTINUAR!"=="S" (
        echo Setup cancelado. Levanta Docker y vuelve a ejecutar setup.bat
        pause
        exit /b 0
    )
    goto skip_migration
)
echo ✓ Postgres está corriendo
echo.

echo [Paso 7] Aplicando migraciones de base de datos...
echo [INFO] Esto creará las tablas en Postgres
echo.
call migrate.bat
if !ERRORLEVEL! NEQ 0 (
    echo ⚠️  Error en migraciones
    echo   Puedes ejecutar migrate.bat manualmente después
    pause
)

:skip_migration
echo.
echo ============================================
echo Setup completado exitosamente!
echo ============================================
echo.
echo ✓ Solución compilada: !SOLUTION_FILE!
echo ✓ Paquetes restaurados
echo.
echo Próximos pasos:
echo.
echo 1. Si no se aplicaron las migraciones:
echo    migrate.bat
echo.
echo 2. Ejecutar API:
echo    run-api.bat
echo.
echo 3. Ejecutar Worker (otra terminal):
echo    run-worker.bat
echo.
echo 4. Abrir Swagger:
echo    http://localhost:5000/swagger
echo.
pause
endlocal
