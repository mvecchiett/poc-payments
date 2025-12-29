@echo off
setlocal enabledelayedexpansion

REM ============================================
REM init.bat - Bootstrap completo del POC
REM Requisitos:
REM - Docker Desktop corriendo
REM - docker compose disponible
REM - migrate.bat, run-api.bat, run-worker.bat existen en raiz
REM ============================================

set ROOT=%~dp0
set INFRA=%ROOT%infra

echo ============================================
echo POC Payments - INIT
echo ROOT: %ROOT%
echo ============================================
echo.

REM --- Validaciones rápidas ---
where docker >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
  echo [ERROR] No se encontro 'docker' en PATH. Instala Docker Desktop o agrega docker al PATH.
  exit /b 1
)

if not exist "%INFRA%\docker-compose.yml" (
  echo [ERROR] No existe "%INFRA%\docker-compose.yml"
  exit /b 1
)

if not exist "%ROOT%migrate.bat" (
  echo [ERROR] No existe "%ROOT%migrate.bat"
  exit /b 1
)

if not exist "%ROOT%run-api.bat" (
  echo [ERROR] No existe "%ROOT%run-api.bat"
  exit /b 1
)

if not exist "%ROOT%run-worker.bat" (
  echo [ERROR] No existe "%ROOT%run-worker.bat"
  exit /b 1
)

REM --- 1) Levantar Docker Compose ---
echo [1/5] Levantando infraestructura (docker compose up -d)...
pushd "%INFRA%" >nul
docker compose up -d
if %ERRORLEVEL% NEQ 0 (
  popd >nul
  echo [ERROR] Fallo 'docker compose up -d'. Verifica Docker Desktop.
  exit /b 1
)
popd >nul
echo [OK] Infra levantada.
echo.

REM --- 2) Esperar healthy (Postgres/Redis) ---
echo [2/5] Esperando contenedores saludables (hasta 60s)...
set /a MAX_SECONDS=60
set /a ELAPSED=0

:WAIT_HEALTH
set PG_HEALTH=
set RD_HEALTH=

for /f "usebackq tokens=*" %%i in (`docker inspect -f "{{.State.Health.Status}}" payments-postgres 2^>nul`) do set PG_HEALTH=%%i
for /f "usebackq tokens=*" %%i in (`docker inspect -f "{{.State.Health.Status}}" payments-redis 2^>nul`) do set RD_HEALTH=%%i

REM Si inspect falla (contenedor sin healthcheck o no existe), igual mostramos estado.
if "%PG_HEALTH%"=="" set PG_HEALTH=unknown
if "%RD_HEALTH%"=="" set RD_HEALTH=unknown

echo    Postgres: %PG_HEALTH%   Redis: %RD_HEALTH%

if /i "%PG_HEALTH%"=="healthy" if /i "%RD_HEALTH%"=="healthy" goto HEALTH_OK

if %ELAPSED% GEQ %MAX_SECONDS% (
  echo [WARN] Timeout esperando healthy. Continuo igual, pero puede fallar migrate/worker.
  goto HEALTH_OK
)

timeout /t 5 /nobreak >nul
set /a ELAPSED+=5
goto WAIT_HEALTH

:HEALTH_OK
echo [OK] Health check finalizado.
echo.

REM --- 3) Migraciones (NO destructivo) ---
echo [3/5] Aplicando migraciones (migrate.bat)...
pushd "%ROOT%" >nul
call "%ROOT%migrate.bat"
if %ERRORLEVEL% NEQ 0 (
  popd >nul
  echo [ERROR] migrate.bat fallo. Revisa salida arriba.
  exit /b 1
)
popd >nul
echo [OK] Migraciones aplicadas.
echo.

REM --- 4) Levantar API en nueva ventana ---
echo [4/5] Iniciando API en nueva ventana...
start "Payments.Api" cmd /k ""%ROOT%run-api.bat""
echo [OK] Ventana API lanzada.
echo.

REM --- 5) Levantar Worker en nueva ventana ---
echo [5/5] Iniciando Worker en nueva ventana...
start "Payments.Worker" cmd /k ""%ROOT%run-worker.bat""
echo [OK] Ventana Worker lanzada.
echo.

echo ============================================
echo INIT completo. Proximo:
echo - Swagger: http://localhost:5000/swagger
echo ============================================
endlocal
exit /b 0
