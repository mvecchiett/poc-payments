@echo off
setlocal

REM ============================================
REM run-front.bat - Ejecuta el Front Next.js
REM Requisitos:
REM - Node.js LTS instalado
REM - npm disponible en PATH
REM - frontend/payments-web existe
REM ============================================

set ROOT=%~dp0
set FRONT=%ROOT%frontend\payments-web

echo ============================================
echo Ejecutando Payments Web (Next.js)
echo ============================================
echo.

REM --- Validaciones ---
where node >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
  echo [ERROR] Node.js no encontrado en PATH.
  echo Instala Node.js LTS y reinicia la consola.
  exit /b 1
)

where npm >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
  echo [ERROR] npm no encontrado en PATH.
  exit /b 1
)

if not exist "%FRONT%\package.json" (
  echo [ERROR] No existe "%FRONT%\package.json"
  exit /b 1
)

REM --- Ir al front ---
pushd "%FRONT%" >nul

REM --- Instalar dependencias si falta node_modules ---
if not exist "node_modules" (
  echo [INFO] node_modules no existe. Ejecutando npm install...
  npm install
  if %ERRORLEVEL% NEQ 0 (
    popd >nul
    echo [ERROR] npm install fallo.
    exit /b 1
  )
  echo [OK] npm install completado.
) else (
  echo [OK] Dependencias ya instaladas.
)

echo.
echo [INFO] Iniciando Frontend...
echo [INFO] URL: http://localhost:3000
echo.

npm run dev

popd >nul
endlocal
exit /b 0
