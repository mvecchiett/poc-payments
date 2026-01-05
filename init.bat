@echo off
setlocal enabledelayedexpansion

REM ============================================
REM init.bat - Bootstrap completo del POC
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
  echo [ERROR] Docker no encontrado en PATH.
  exit /b 1
)

if not exist "%INFRA%\docker-compose.yml" (
  echo [ERROR] No existe docker-compose.yml en infra
  exit /b 1
)

if not exist "%ROOT%run-front.bat" (
  echo [ERROR] No existe "%ROOT%run-front.bat"
  exit /b 1
)

REM --- 1) Levantar Docker ---
echo [1/6] Levantando infraestructura (docker compose up -d)...
pushd "%INFRA%" >nul
docker compose up -d
if %ERRORLEVEL% NEQ 0 (
  popd >nul
  echo [ERROR] docker compose fallo.
  exit /b 1
)
popd >nul
echo [OK] Infra levantada.
echo.

REM --- 2) Esperar healthy ---
echo [2/6] Esperando contenedores saludables...
set /a MAX_SECONDS=60
set /a ELAPSED=0

:WAIT_HEALTH
set PG_HEALTH=
set RD_HEALTH=

for /f "usebackq tokens=*" %%i in (`docker inspect -f "{{.State.Health.Status}}" payments-postgres 2^>nul`) do set PG_HEALTH=%%i
for /f "usebackq tokens=*" %%i in (`docker inspect -f "{{.State.Health.Status}}" payments-redis 2^>nul`) do set RD_HEALTH=%%i

if "%PG_HEALTH%"=="" set PG_HEALTH=unknown
if "%RD_HEALTH%"=="" set RD_HEALTH=unknown

echo    Postgres: %PG_HEALTH%   Redis: %RD_HEALTH%

if /i "%PG_HEALTH%"=="healthy" if /i "%RD_HEALTH%"=="healthy" goto HEALTH_OK

if %ELAPSED% GEQ %MAX_SECONDS% (
  echo [WARN] Timeout esperando healthchecks.
  goto HEALTH_OK
)

timeout /t 5 /nobreak >nul
set /a ELAPSED+=5
goto WAIT_HEALTH

:HEALTH_OK
echo [OK] Health check finalizado.
echo.

REM --- 3) Migraciones ---
echo [3/6] Aplicando migraciones...
call "%ROOT%migrate.bat"
if %ERRORLEVEL% NEQ 0 (
  echo [ERROR] migrate.bat fallo.
  exit /b 1
)
echo [OK] Migraciones aplicadas.
echo.

REM --- 4) API ---
echo [4/6] Iniciando API...
start "Payments.Api" cmd /k ""%ROOT%run-api.bat""
echo [OK] API lanzada.
echo.

REM --- 5) Worker ---
echo [5/6] Iniciando Worker...
start "Payments.Worker" cmd /k ""%ROOT%run-worker.bat""
echo [OK] Worker lanzado.
echo.

REM --- 6) Levantar Front en nueva ventana ---
echo [6/6] Iniciando Frontend (Next.js) en nueva ventana...
start "Payments.Web" cmd /k ""%ROOT%run-front.bat""
echo [OK] Ventana Front lanzada.
echo.

echo.
echo ============================================
echo INIT completo
echo API     : http://localhost:5000/swagger
echo Front   : http://localhost:3000

echo ============================================

endlocal
exit /b 0
