# Cambios de Dominio - Fase 1B (Ajustes)

## Resumen de mejoras implementadas

### 1. Expiración automática real ✅

**Qué cambió:**
- El Worker ahora expira intents de verdad (no solo loguea)
- Nuevo método `ExpirePendingAsync()` en el servicio
- Busca intents en `PendingConfirmation` que pasaron su tiempo de expiración

**Flujo:**
```
Worker (cada 30s) 
  → Llama a ExpirePendingAsync()
  → Busca: Status=PendingConfirmation AND ExpiresAt <= Now
  → Actualiza a: Status=Expired, ExpiredAt=Now
```

---

### 2. Campo ExpiresAt explícito ✅

**Qué cambió:**
- Nuevo campo `ExpiresAt` (DateTime nullable) en `PaymentIntent`
- Se calcula al confirmar: `ExpiresAt = ConfirmedAt + 120 segundos`
- El Worker usa solo este campo (no calcula)

**Ventajas:**
- Más eficiente: el Worker no calcula, solo compara
- Más claro: la expiración es explícita y visible
- Más testeable: podés setear `ExpiresAt` manualmente en tests

**Response API:**
```json
{
  "id": "pi_abc123",
  "status": "PendingConfirmation",
  "confirmedAt": "2025-12-22T20:00:00Z",
  "expiresAt": "2025-12-22T20:02:00Z",  // ← NUEVO
  ...
}
```

---

### 3. HTTP 409 Conflict ✅

**Qué cambió:**
- Transiciones inválidas retornan `409 Conflict` en lugar de `400 Bad Request`

**Ejemplos:**
- Intentar capturar sin confirmar → **409 Conflict**
- Intentar confirmar un intent ya capturado → **409 Conflict**
- Intentar revertir un intent expirado → **409 Conflict**

**Por qué es mejor:**
- `400` → Error en el request (datos inválidos)
- `409` → Recurso existe pero hay conflicto de estado (más preciso)

---

## Migración de base de datos

**Nueva columna:**
```sql
ALTER TABLE payment_intents 
ADD COLUMN expires_at TIMESTAMP NULL;

CREATE INDEX ix_payment_intents_expires_at 
ON payment_intents(expires_at);
```

**Aplicar:**
```bash
cd src\Payments.Api
dotnet ef migrations add AddExpiresAtField
dotnet ef database update
```

---

## Flujo completo actualizado

### Crear → Confirmar → Expirar (automático)

```bash
# 1. Crear intent
POST /api/payment-intents
→ Status: Created

# 2. Confirmar intent
POST /api/payment-intents/{id}/confirm
→ Status: PendingConfirmation
→ ExpiresAt: ConfirmedAt + 120s

# 3. Esperar 2+ minutos (Worker automático)
→ Worker detecta ExpiresAt <= Now
→ Status: Expired
```

### Crear → Confirmar → Capturar (antes de expirar)

```bash
# 1-2. Crear y confirmar (igual)

# 3. Capturar antes de que expire
POST /api/payment-intents/{id}/capture
→ Status: Captured

# Worker ya no puede expirar (estado final)
```

---

## Testing

### Caso 1: Expiración automática
```bash
# Crear y confirmar
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":10000,"currency":"ARS"}'

curl -X POST http://localhost:5000/api/payment-intents/{id}/confirm

# Ver ExpiresAt en la respuesta
# Esperar 2+ minutos
# Consultar nuevamente → debería estar Expired
```

### Caso 2: Capturar antes de expirar
```bash
# Crear, confirmar y capturar rápido (< 2 min)
# El Worker no lo expirará porque ya está Captured
```

### Caso 3: Conflicto 409
```bash
# Crear intent
curl -X POST http://localhost:5000/api/payment-intents/...

# Intentar capturar SIN confirmar → 409 Conflict
curl -X POST http://localhost:5000/api/payment-intents/{id}/capture
# Response: {"error": "Cannot capture payment intent in status Created..."}
```

---

## Logs esperados

### API (confirmación)
```
info: Payment intent confirmed: pi_abc123, expires at: 2025-12-22T20:02:00Z
```

### Worker (expiración)
```
info: Worker running at: 12/22/2025 8:02:15 PM +00:00
info: Payment intent expired: pi_abc123, was pending since 12/22/2025 8:00:00 PM
info: Expired 1 payment intents
```

---

## Próximos pasos

Con estos ajustes, el flujo completo está implementado y listo para testear:

1. ✅ Persistencia en Postgres
2. ✅ Validación de transiciones
3. ✅ Expiración automática funcional
4. ✅ HTTP status codes correctos
5. ⏳ Pendiente: Idempotencia (Fase 3)
6. ⏳ Pendiente: Outbox pattern (Fase 4)
