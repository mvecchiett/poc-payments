@echo off
echo ============================================
echo ⚠️  RESET COMPLETO DE BASE DE DATOS ⚠️
echo ============================================
echo.
echo [ADVERTENCIA] Este script es DESTRUCTIVO
echo [ADVERTENCIA] Eliminara TODOS los datos de la base de datos
echo [ADVERTENCIA] Solo usar en desarrollo/testing
echo.
echo Presiona Ctrl+C para cancelar
pause
echo.

cd /d "%~dp0src\Payments.Api"

echo [1] Eliminando base de datos...
dotnet ef database drop --force
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error al eliminar base de datos
    echo (Esto es normal si la DB no existe todavia)
)
echo ✓ Base de datos eliminada
echo.

echo [2] Eliminando migraciones anteriores...
if exist "Migrations" (
    rmdir /s /q "Migrations"
    echo ✓ Carpeta Migrations eliminada
) else (
    echo ✓ No hay carpeta Migrations
)
echo.

echo [3] Creando migracion inicial...
dotnet ef migrations add InitialCreateWithExpiresAt
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error creando migracion
    pause
    exit /b 1
)
echo ✓ Migracion creada
echo.

echo [4] Aplicando migracion...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error aplicando migracion
    pause
    exit /b 1
)
echo ✓ Base de datos creada desde cero
echo.

echo ============================================
echo Reset completado exitosamente
echo ============================================
echo.
echo La base de datos esta limpia y lista.
echo.
pause
