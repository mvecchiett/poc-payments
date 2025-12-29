@echo off
echo ============================================
echo Safe Refactor - Limpieza de duplicados
echo ============================================
echo.

REM Guardar el directorio actual
pushd "%~dp0"

cd /d "%~dp0src\Payments.Api"

echo [1] Verificando carpetas duplicadas en API...
if exist "Data" (
    echo ✓ Encontrada carpeta Data (duplicado)
    rmdir /s /q "Data"
    echo ✓ Carpeta Data eliminada
) else (
    echo ✓ No hay carpeta Data
)

if exist "Services" (
    echo ✓ Encontrada carpeta Services (duplicado)
    rmdir /s /q "Services"
    echo ✓ Carpeta Services eliminada
) else (
    echo ✓ No hay carpeta Services
)

echo.
echo [2] Verificando estructura de Application...
cd /d "%~dp0src\Payments.Application"

if exist "Data\PaymentsDbContext.cs" (
    echo ✓ PaymentsDbContext existe en Application
) else (
    echo ✗ FALTA PaymentsDbContext en Application
    popd
    pause
    exit /b 1
)

if exist "Services\PaymentIntentService.cs" (
    echo ✓ PaymentIntentService existe en Application
) else (
    echo ✗ FALTA PaymentIntentService en Application
    popd
    pause
    exit /b 1
)

echo.
echo ============================================
echo Limpieza completada exitosamente
echo ============================================
echo.

REM Volver al directorio original
popd

REM NO hacer pause aquí, dejar que setup.bat continúe
