# Casos de Prueba - Fase 1B
## Payment Intent POC - Validación Funcional Completa

---

## 📋 Prerrequisitos

Antes de ejecutar las pruebas, verificar:

- ✅ Docker corriendo (Postgres + Redis)
- ✅ Tablas creadas en Postgres (`payment_intents`, `__EFMigrationsHistory`)
- ✅ API corriendo en http://localhost:5000
- ✅ Worker corriendo (loguea cada 30 segundos)
- ✅ Swagger disponible en http://localhost:5000/swagger

---

## 🧪 CASO PRINCIPAL: Flujo Completo de Expiración

### Objetivo
Validar que un Payment Intent se crea, se confirma, y expira automáticamente después de 2 minutos.

### Pasos

#### 1. Health Check
**Endpoint:** `GET /api/health`

**Resultado esperado:**
```json
{
  "status": "healthy",
  "timestamp": "2025-12-23T..."
}
```

---

#### 2. Crear Payment Intent
**Endpoint:** `POST /api/payment-intents`

**Request Body:**
```json
{
  "amount": 10000,
  "currency": "ARS",
  "description": "Test expiración automática"
}
```

**Resultado esperado:**
- Status: `201 Created`
- Response:
```json
{
  "id": "pi_abc123...",
  "status": "Created",
  "amount": 10000,
  "currency": "ARS",
  "description": "Test expiración automática",
  "createdAt": "2025-12-23T20:00:00Z",
  "updatedAt": "2025-12-23T20:00:00Z",
  "confirmedAt": null,
  "expiresAt": null,
  "capturedAt": null,
  "reversedAt": null,
  "expiredAt": null
}
```

**📝 Acción:** Copiar el `id` del response

---

#### 3. Confirmar Payment Intent
**Endpoint:** `POST /api/payment-intents/{id}/confirm`

Pegar el `id` copiado en el paso anterior.

**Resultado esperado:**
- Status: `200 OK`
- Response:
```json
{
  "id": "pi_abc123...",
  "status": "PendingConfirmation",
  "confirmedAt": "2025-12-23T20:00:00Z",
  "expiresAt": "2025-12-23T20:02:00Z",  // confirmedAt + 120 segundos
  "updatedAt": "2025-12-23T20:00:00Z",
  ...
}
```

**✅ Verificaciones:**
- `status` cambió de `Created` → `PendingConfirmation`
- `confirmedAt` está seteado
- `expiresAt` está seteado (confirmedAt + 2 minutos)
- `expiresAt` NO es null

**⏰ Acción:** Anotar la hora de `expiresAt`

---

#### 4. Esperar expiración automática (2+ minutos)

**No hacer nada durante 2+ minutos.**

**En la terminal del Worker (después de ~2 minutos):**
```
info: Worker running at: 2025-12-23T20:02:30Z
info: Payment intent expired: pi_abc123..., was pending since 2025-12-23T20:00:00Z
info: Expired 1 payment intents
```

**✅ Verificaciones:**
- El Worker loguea que expiró el intent
- El timestamp del log es >= `expiresAt`

---

#### 5. Verificar estado final
**Endpoint:** `GET /api/payment-intents/{id}`

**Resultado esperado:**
- Status: `200 OK`
- Response:
```json
{
  "id": "pi_abc123...",
  "status": "Expired",
  "confirmedAt": "2025-12-23T20:00:00Z",
  "expiresAt": null,                      // ← Limpiado
  "expiredAt": "2025-12-23T20:02:30Z",    // ← Seteado por el Worker
  "updatedAt": "2025-12-23T20:02:30Z",
  ...
}
```

**✅ Verificaciones:**
- `status` cambió de `PendingConfirmation` → `Expired`
- `expiredAt` está seteado
- `expiresAt` está limpiado (null)
- `updatedAt` refleja el momento de la expiración

---

#### 6. Verificar en base de datos

```bash
docker exec -it payments-postgres psql -U postgres -d payments_db
```

```sql
SELECT id, status, confirmed_at, expires_at, expired_at, updated_at
FROM payment_intents 
WHERE id = 'pi_abc123...'  -- Reemplazar con el ID real
ORDER BY created_at DESC;
```

**Resultado esperado:**
```
id        | status  | confirmed_at        | expires_at | expired_at          | updated_at
----------+---------+--------------------+------------+--------------------+--------------------
pi_abc... | Expired | 2025-12-23 20:00:00| null       | 2025-12-23 20:02:30| 2025-12-23 20:02:30
```

**✅ Verificaciones:**
- Estado en DB = `Expired`
- `expires_at` = null
- `expired_at` tiene timestamp

---

### ✅ Resultado del Caso Principal
- [ ] Intent creado correctamente
- [ ] Intent confirmado correctamente
- [ ] `expiresAt` seteado al confirmar
- [ ] Worker expiró el intent automáticamente después de 2 min
- [ ] Estado final = `Expired`
- [ ] `expiresAt` limpiado (null)
- [ ] `expiredAt` seteado
- [ ] Base de datos refleja el estado correcto

---

## 🧪 TEST A: Capturar Antes de Expirar

### Objetivo
Validar que un intent capturado NO expira, incluso si pasan 2+ minutos.

### Pasos

1. **Crear intent:** `POST /api/payment-intents`
   - Body: `{"amount": 5000, "currency": "ARS", "description": "Test captura"}`
   - Copiar `id`

2. **Confirmar intent:** `POST /api/payment-intents/{id}/confirm`
   - Verificar que `expiresAt` está seteado

3. **Capturar INMEDIATAMENTE (< 2 min):** `POST /api/payment-intents/{id}/capture`

**Resultado esperado:**
- Status: `200 OK`
- Response:
```json
{
  "id": "pi_...",
  "status": "Captured",
  "capturedAt": "2025-12-23T20:01:00Z",
  "expiresAt": null,  // ← Limpiado al capturar
  ...
}
```

4. **Esperar 2+ minutos** (sin hacer nada)

5. **Verificar en terminal del Worker:**
   - El Worker NO debe loguear que expiró este intent
   - Solo expira intents en estado `PendingConfirmation`

6. **Consultar estado:** `GET /api/payment-intents/{id}`

**Resultado esperado:**
```json
{
  "status": "Captured",  // ← NO cambió a Expired
  "expiresAt": null,
  "expiredAt": null,     // ← NO se seteó
  ...
}
```

### ✅ Resultado del Test A
- [ ] Intent capturado exitosamente
- [ ] `expiresAt` limpiado al capturar
- [ ] Worker NO expiró el intent
- [ ] Estado permanece `Captured` después de 2+ minutos

---

## 🧪 TEST B: Capturar Sin Confirmar (409 Conflict)

### Objetivo
Validar que no se puede capturar un intent sin confirmarlo primero.

### Pasos

1. **Crear intent:** `POST /api/payment-intents`
   - Body: `{"amount": 3000, "currency": "ARS"}`
   - Copiar `id`

2. **Intentar capturar SIN confirmar:** `POST /api/payment-intents/{id}/capture`

**Resultado esperado:**
- Status: `409 Conflict`
- Response:
```json
{
  "error": "Cannot capture payment intent in status Created. Must be in PendingConfirmation status."
}
```

3. **Verificar estado:** `GET /api/payment-intents/{id}`

**Resultado esperado:**
```json
{
  "status": "Created",  // ← NO cambió
  ...
}
```

### ✅ Resultado del Test B
- [ ] Captura sin confirmar retorna `409 Conflict`
- [ ] Mensaje de error es descriptivo
- [ ] Estado permanece `Created`

---

## 🧪 TEST C: Confirmar Dos Veces (409 Conflict)

### Objetivo
Validar que no se puede confirmar un intent dos veces.

### Pasos

1. **Crear intent:** `POST /api/payment-intents`
   - Body: `{"amount": 7500, "currency": "ARS"}`
   - Copiar `id`

2. **Confirmar intent:** `POST /api/payment-intents/{id}/confirm`
   - Verificar que retorna `200 OK`

3. **Intentar confirmar DE NUEVO:** `POST /api/payment-intents/{id}/confirm`

**Resultado esperado:**
- Status: `409 Conflict`
- Response:
```json
{
  "error": "Cannot confirm payment intent in status PendingConfirmation. Must be in Created status."
}
```

4. **Verificar estado:** `GET /api/payment-intents/{id}`

**Resultado esperado:**
```json
{
  "status": "PendingConfirmation",  // ← NO cambió
  ...
}
```

### ✅ Resultado del Test C
- [ ] Segunda confirmación retorna `409 Conflict`
- [ ] Mensaje de error es descriptivo
- [ ] Estado permanece `PendingConfirmation`

---

## 🧪 TEST D: Revertir Manualmente

### Objetivo
Validar que se puede revertir un intent y que el Worker no lo expira después.

### Pasos

1. **Crear intent:** `POST /api/payment-intents`
   - Body: `{"amount": 12000, "currency": "ARS", "description": "Test reversa"}`
   - Copiar `id`

2. **Confirmar intent:** `POST /api/payment-intents/{id}/confirm`
   - Verificar que `expiresAt` está seteado

3. **Revertir ANTES de que expire (< 2 min):** `POST /api/payment-intents/{id}/reverse`

**Resultado esperado:**
- Status: `200 OK`
- Response:
```json
{
  "id": "pi_...",
  "status": "Reversed",
  "reversedAt": "2025-12-23T20:01:30Z",
  "expiresAt": null,  // ← Limpiado al revertir
  ...
}
```

4. **Esperar 2+ minutos**

5. **Verificar en terminal del Worker:**
   - El Worker NO debe loguear que expiró este intent
   - Solo expira intents en estado `PendingConfirmation`

6. **Consultar estado:** `GET /api/payment-intents/{id}`

**Resultado esperado:**
```json
{
  "status": "Reversed",  // ← NO cambió a Expired
  "expiresAt": null,
  "expiredAt": null,     // ← NO se seteó
  ...
}
```

### ✅ Resultado del Test D
- [ ] Intent revertido exitosamente
- [ ] `expiresAt` limpiado al revertir
- [ ] Worker NO expiró el intent
- [ ] Estado permanece `Reversed` después de 2+ minutos

---

## 🧪 TEST E: Revertir Desde Created

### Objetivo
Validar que se puede revertir un intent sin confirmarlo primero.

### Pasos

1. **Crear intent:** `POST /api/payment-intents`
   - Body: `{"amount": 2500, "currency": "ARS"}`
   - Copiar `id`

2. **Revertir SIN confirmar:** `POST /api/payment-intents/{id}/reverse`

**Resultado esperado:**
- Status: `200 OK`
- Response:
```json
{
  "id": "pi_...",
  "status": "Reversed",
  "reversedAt": "2025-12-23T20:00:30Z",
  "confirmedAt": null,   // ← Nunca se confirmó
  "expiresAt": null,
  ...
}
```

### ✅ Resultado del Test E
- [ ] Reversa desde `Created` funciona correctamente
- [ ] Estado cambió de `Created` → `Reversed`
- [ ] `confirmedAt` sigue siendo null

---

## 🧪 TEST F: Capturar Intent Expirado (409 Conflict)

### Objetivo
Validar que no se puede capturar un intent que ya expiró.

### Pasos

1. **Crear intent:** `POST /api/payment-intents`
   - Body: `{"amount": 4500, "currency": "ARS"}`
   - Copiar `id`

2. **Confirmar intent:** `POST /api/payment-intents/{id}/confirm`

3. **Esperar 2+ minutos** (dejar que expire)

4. **Verificar que expiró en Worker logs:**
   ```
   info: Payment intent expired: pi_...
   ```

5. **Intentar capturar:** `POST /api/payment-intents/{id}/capture`

**Resultado esperado:**
- Status: `409 Conflict`
- Response:
```json
{
  "error": "Cannot capture payment intent in status Expired. Must be in PendingConfirmation status."
}
```

### ✅ Resultado del Test F
- [ ] Captura de intent expirado retorna `409 Conflict`
- [ ] Mensaje de error es descriptivo
- [ ] Estado permanece `Expired`

---

## 🧪 TEST G: Revertir Intent Capturado (409 Conflict)

### Objetivo
Validar que no se puede revertir un intent que ya fue capturado.

### Pasos

1. **Crear y confirmar intent**

2. **Capturar intent:** `POST /api/payment-intents/{id}/capture`
   - Verificar que estado = `Captured`

3. **Intentar revertir:** `POST /api/payment-intents/{id}/reverse`

**Resultado esperado:**
- Status: `409 Conflict`
- Response:
```json
{
  "error": "Cannot reverse payment intent in status Captured. Must be in Created or PendingConfirmation status."
}
```

### ✅ Resultado del Test G
- [ ] Reversa de intent capturado retorna `409 Conflict`
- [ ] Mensaje de error es descriptivo
- [ ] Estado permanece `Captured`

---

## 🧪 TEST H: Múltiples Intents Expiran Simultáneamente

### Objetivo
Validar que el Worker puede expirar múltiples intents en una sola ejecución.

### Pasos

1. **Crear y confirmar 3 intents rápidamente:**
   - Intent 1: amount 1000
   - Intent 2: amount 2000
   - Intent 3: amount 3000
   - Confirmar los 3 en menos de 10 segundos

2. **Esperar 2+ minutos**

3. **Verificar en terminal del Worker:**

**Resultado esperado:**
```
info: Worker running at: 2025-12-23T20:02:30Z
info: Payment intent expired: pi_001..., was pending since ...
info: Payment intent expired: pi_002..., was pending since ...
info: Payment intent expired: pi_003..., was pending since ...
info: Expired 3 payment intents
```

4. **Consultar los 3 intents:**
   - `GET /api/payment-intents/{id1}`
   - `GET /api/payment-intents/{id2}`
   - `GET /api/payment-intents/{id3}`

**Resultado esperado:**
- Los 3 deben tener `status`: "Expired"

### ✅ Resultado del Test H
- [ ] Worker expiró múltiples intents en una ejecución
- [ ] Los 3 intents quedaron en estado `Expired`
- [ ] Log muestra "Expired 3 payment intents"

---

## 📊 Comandos SQL Útiles para Verificación

### Ver todos los intents

```sql
SELECT id, status, amount, created_at, confirmed_at, expires_at, expired_at
FROM payment_intents
ORDER BY created_at DESC
LIMIT 10;
```

---

### Ver solo intents expirados

```sql
SELECT id, status, confirmed_at, expired_at, 
       EXTRACT(EPOCH FROM (expired_at - confirmed_at)) as seconds_until_expiration
FROM payment_intents
WHERE status = 'Expired'
ORDER BY expired_at DESC;
```

---

### Ver distribución de estados

```sql
SELECT status, COUNT(*) as count
FROM payment_intents
GROUP BY status
ORDER BY count DESC;
```

---

### Ver intents que están por expirar

```sql
SELECT id, status, expires_at,
       EXTRACT(EPOCH FROM (expires_at - NOW())) as seconds_remaining
FROM payment_intents
WHERE status = 'PendingConfirmation'
  AND expires_at IS NOT NULL
ORDER BY expires_at ASC;
```

---

### Verificar que expires_at se limpia correctamente

```sql
SELECT status, 
       COUNT(*) as total,
       SUM(CASE WHEN expires_at IS NULL THEN 1 ELSE 0 END) as expires_at_null
FROM payment_intents
WHERE status IN ('Captured', 'Reversed', 'Expired')
GROUP BY status;
```

**Resultado esperado:**
```
status   | total | expires_at_null
---------+-------+----------------
Captured |     X |       X         (todos deben ser NULL)
Reversed |     Y |       Y         (todos deben ser NULL)
Expired  |     Z |       Z         (todos deben ser NULL)
```

---

## 🎯 Checklist Final - Validación Completa Fase 1B

### Funcionalidad Core
- [ ] ✅ Crear intent (estado inicial: Created)
- [ ] ✅ Confirmar intent (Created → PendingConfirmation)
- [ ] ✅ Capturar intent (PendingConfirmation → Captured)
- [ ] ✅ Revertir intent (Created/PendingConfirmation → Reversed)
- [ ] ✅ Expiración automática (PendingConfirmation → Expired)

### Validaciones de Estado
- [ ] ✅ No capturar sin confirmar (409 Conflict)
- [ ] ✅ No confirmar dos veces (409 Conflict)
- [ ] ✅ No capturar intent expirado (409 Conflict)
- [ ] ✅ No revertir intent capturado (409 Conflict)

### Expiración
- [ ] ✅ `expiresAt` se setea al confirmar (confirmedAt + 120s)
- [ ] ✅ Worker expira intents automáticamente
- [ ] ✅ Worker NO expira intents Captured
- [ ] ✅ Worker NO expira intents Reversed
- [ ] ✅ `expiresAt` se limpia en estados finales
- [ ] ✅ `expiredAt` se setea al expirar
- [ ] ✅ Worker puede expirar múltiples intents simultáneamente

### Base de Datos
- [ ] ✅ Tabla `payment_intents` existe
- [ ] ✅ Todos los campos están correctos
- [ ] ✅ Índices creados (status, created_at, expires_at)
- [ ] ✅ Timestamps se guardan correctamente
- [ ] ✅ Estados se reflejan correctamente en DB

### Arquitectura
- [ ] ✅ API funciona correctamente
- [ ] ✅ Worker funciona correctamente
- [ ] ✅ Worker NO depende de API
- [ ] ✅ Worker loguea cada 30 segundos
- [ ] ✅ HTTP 409 Conflict para transiciones inválidas
- [ ] ✅ HTTP 404 Not Found para intents inexistentes
- [ ] ✅ HTTP 201 Created al crear intent

---

## 📝 Notas de Ejecución

**Fecha de ejecución:** _________________

**Ejecutado por:** _________________

**Resultados:**
- Tests exitosos: _____ / _____
- Tests fallidos: _____
- Observaciones:

---

## 🚀 Próximos Pasos (Post Fase 1B)

Una vez que TODOS los tests estén ✅:

**FASE 1B CERRADA**

Continuar con:
- **Fase 2:** Idempotencia con Redis (Idempotency-Key header)
- **Fase 3:** Outbox pattern para eventos
- **Fase 4:** Observabilidad (CorrelationId, métricas, OpenTelemetry)
- **Fase 5:** Frontend intercambiable (Next.js SSR dashboard)
