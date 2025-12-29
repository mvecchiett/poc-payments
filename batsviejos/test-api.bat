@echo off
echo ============================================
echo Payment Intent POC - Quick Test Script
echo ============================================
echo.

echo [1] Testing Health Endpoint...
curl -X GET http://localhost:5000/api/health
echo.
echo.

echo [2] Creating Payment Intent...
curl -X POST http://localhost:5000/api/payment-intents ^
  -H "Content-Type: application/json" ^
  -d "{\"amount\":10000,\"currency\":\"ARS\",\"description\":\"Test payment\"}"
echo.
echo.

echo.
echo Enter the Payment Intent ID to continue testing (from the response above):
set /p INTENT_ID=Payment Intent ID: 

echo.
echo [3] Getting Payment Intent %INTENT_ID%...
curl -X GET http://localhost:5000/api/payment-intents/%INTENT_ID%
echo.
echo.

echo [4] Confirming Payment Intent %INTENT_ID%...
curl -X POST http://localhost:5000/api/payment-intents/%INTENT_ID%/confirm
echo.
echo.

echo [5] Capturing Payment Intent %INTENT_ID%...
curl -X POST http://localhost:5000/api/payment-intents/%INTENT_ID%/capture
echo.
echo.

echo ============================================
echo Test completed!
echo ============================================
pause
