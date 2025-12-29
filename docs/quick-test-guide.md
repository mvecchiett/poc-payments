# Quick Test Guide - Fase 1B
## Guía Rápida de Pruebas

Este documento contiene pruebas rápidas para validar la funcionalidad básica.

Para casos de prueba completos, ver: `test-cases-phase1b.md`

---

## ⚡ Test Rápido Principal (5 minutos)

### 1. Health Check
```bash
curl http://localhost:5000/api/health
```
✅ Debe retornar `{"status":"healthy",...}`

---

### 2. Crear Intent
```bash
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":10000,"currency":"ARS","description":"Test"}'
```
✅ Status: 201 Created
📝 Copiar el `id` del response

---

### 3. Confirmar Intent
```bash
curl -X POST http://localhost:5000/api/payment-intents/{ID}/confirm
```
Reemplazar `{ID}` con el id copiado.

✅ Status: 200 OK
✅ Response debe tener:
- `"status":"PendingConfirmation"`
- `"expiresAt"` NO null

---

### 4. Esperar 2+ minutos

En la terminal del Worker debe aparecer:
```
info: Payment intent expired: pi_...
info: Expired 1 payment intents
```

---

### 5. Verificar Estado
```bash
curl http://localhost:5000/api/payment-intents/{ID}
```

✅ Response debe tener:
- `"status":"Expired"`
- `"expiresAt": null`
- `"expiredAt"` NO null

---

## ⚡ Test Rápido: Transición Inválida (1 minuto)

### 1. Crear intent
```bash
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":5000,"currency":"ARS"}'
```
📝 Copiar el `id`

### 2. Intentar capturar SIN confirmar
```bash
curl -X POST http://localhost:5000/api/payment-intents/{ID}/capture
```

✅ Status: 409 Conflict
✅ Response: `{"error":"Cannot capture..."}`

---

## ⚡ Test Rápido: Captura Exitosa (1 minuto)

### 1. Crear y confirmar intent
```bash
# Crear
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":3000,"currency":"ARS"}'

# Confirmar (reemplazar {ID})
curl -X POST http://localhost:5000/api/payment-intents/{ID}/confirm
```

### 2. Capturar INMEDIATAMENTE
```bash
curl -X POST http://localhost:5000/api/payment-intents/{ID}/capture
```

✅ Status: 200 OK
✅ Response: `"status":"Captured"`, `"expiresAt": null`

### 3. Esperar 2+ minutos

✅ Worker NO debe expirar este intent (ya está Captured)

---

## 📊 Verificación en Base de Datos

```bash
docker exec -it payments-postgres psql -U postgres -d payments_db
```

### Ver últimos 5 intents
```sql
SELECT id, status, confirmed_at, expires_at, expired_at
FROM payment_intents
ORDER BY created_at DESC
LIMIT 5;
```

### Ver distribución de estados
```sql
SELECT status, COUNT(*) FROM payment_intents GROUP BY status;
```

### Salir
```sql
\q
```

---

## ✅ Checklist Mínimo

- [ ] Health check funciona
- [ ] Crear intent funciona (201 Created)
- [ ] Confirmar intent funciona (200 OK, expiresAt seteado)
- [ ] Worker expira intent automáticamente
- [ ] Estado final = Expired
- [ ] Capturar sin confirmar = 409 Conflict
- [ ] Captura exitosa NO expira

---

## 🚀 Comandos Útiles

### Levantar infraestructura
```bash
cd infra
docker compose up -d
```

### Ver logs del Worker
```bash
docker logs payments-worker -f
```

### Reiniciar API
```bash
# Ctrl+C en la terminal del API
run-api.bat
```

### Ver todos los intents
```bash
curl http://localhost:5000/api/payment-intents
```
(Si implementas el endpoint GET all)

---

Para casos de prueba completos y detallados, consultar: **`test-cases-phase1b.md`**
