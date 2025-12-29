@echo off
echo ============================================
echo Ejecutando Payments API
echo ============================================
echo.

cd /d "%~dp0src\Payments.Api"

echo [INFO] La API se ejecutara en http://localhost:5000
echo [INFO] Swagger estara disponible en http://localhost:5000/swagger
echo [INFO] Presiona Ctrl+C para detener
echo.

dotnet run

pause
