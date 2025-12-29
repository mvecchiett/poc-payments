@echo off
setlocal enabledelayedexpansion
echo ============================================
echo Compilacion completa del POC
echo ============================================
echo.

cd /d "%~dp0"

REM Buscar archivo .sln
FOR %%F IN (*.sln) DO SET SOLUTION_FILE=%%F

if "!SOLUTION_FILE!"=="" (
    echo ✗ ERROR: No se encuentra archivo .sln
    pause
    exit /b 1
)

echo [INFO] Solución: !SOLUTION_FILE!
echo.

echo [1] Limpiando build anterior...
dotnet clean "!SOLUTION_FILE!"
echo ✓ Limpieza completada
echo.

echo [2] Restaurando paquetes...
dotnet restore "!SOLUTION_FILE!"
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Error restaurando paquetes
    pause
    exit /b 1
)
echo ✓ Paquetes restaurados
echo.

echo [3] Compilando solución...
dotnet build "!SOLUTION_FILE!" --no-restore
if !ERRORLEVEL! NEQ 0 (
    echo ✗ Error compilando
    pause
    exit /b 1
)
echo ✓ Compilación exitosa
echo.

echo ============================================
echo Build completado sin errores
echo ============================================
echo.
pause
endlocal
