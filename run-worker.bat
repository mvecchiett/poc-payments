@echo off
echo ============================================
echo Ejecutando Payments Worker
echo ============================================
echo.

cd /d "%~dp0src\Payments.Worker"

echo [INFO] El Worker expirara intents cada 30 segundos
echo [INFO] Presiona Ctrl+C para detener
echo.

dotnet run

pause
