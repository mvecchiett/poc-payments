@echo off
echo ============================================
echo Fix Tailwind CSS - Downgrade a v3
echo ============================================
echo.

cd /d "%~dp0"

echo [1] Limpiando instalacion anterior...
if exist "node_modules" (
    echo Eliminando node_modules...
    rmdir /s /q "node_modules"
)

if exist "package-lock.json" (
    echo Eliminando package-lock.json...
    del package-lock.json
)

if exist ".next" (
    echo Eliminando .next...
    rmdir /s /q ".next"
)
echo ✓ Limpieza completada
echo.

echo [2] Instalando dependencias (Tailwind 3)...
call npm install
if %ERRORLEVEL% NEQ 0 (
    echo ✗ Error instalando dependencias
    pause
    exit /b 1
)
echo ✓ Dependencias instaladas
echo.

echo [3] Verificando instalacion...
call npm list tailwindcss
echo.

echo ============================================
echo Fix completado!
echo ============================================
echo.
echo Ahora ejecuta:
echo   npm run dev
echo.
echo Y abre: http://localhost:3000
echo.
pause
