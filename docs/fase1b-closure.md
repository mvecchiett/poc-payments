# Fase 1B - Cierre y Validación

## ✅ Checklist de Implementación

### 1. Mapeo EF Core ✓
- [x] Todas las propiedades mapeadas a snake_case
- [x] `expires_at` correctamente configurado
- [x] Índices en: `status`, `created_at`, `expires_at`
- [x] Conversión de enum a string

### 2. HTTP Status Codes ✓
- [x] `409 Conflict` para transiciones inválidas (todos los endpoints)
- [x] `404 Not Found` para recursos inexistentes
- [x] `201 Created` al crear intent
- [x] `200 OK` para operaciones exitosas

### 3. Arquitectura Desacoplada ✓
- [x] Nuevo proyecto: `Payments.Application`
- [x] `PaymentsDbContext` movido a Application
- [x] `IPaymentIntentService` y `PaymentIntentService` en Application
- [x] API depende de Application (no viceversa)
- [x] Worker depende de Application (no de API)
- [x] Separación clara: HTTP ≠ Background

**Estructura final:**
```
Payments.Shared       (Modelos, DTOs, Enums)
Payments.Application  (DbContext, Services, Lógica de negocio)
Payments.Api          (Controllers, HTTP endpoints)
Payments.Worker       (Background services)
```

### 4. Limpieza de Dominio ✓
- [x] `ExpiresAt = null` al capturar
- [x] `ExpiresAt = null` al revertir
- [x] `ExpiresAt = null` al expirar
- [x] Estado final = no puede expirar

### 5. Migración Limpia ✓
Script: `migrate-phase1b.bat`
- Elimina DB anterior
- Crea migración inicial completa
- Aplica a Postgres
- Sin warnings ni errores

---

## 🧪 Validación Funcional Requerida

### Test 1: Crear → Confirmar → Expirar (automático)
```bash
# Terminal 1: API
cd src/Payments.Api
dotnet run

# Terminal 2: Worker
cd src/Payments.Worker
dotnet run

# Terminal 3: Pruebas
# 1. Crear intent
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":10000,"currency":"ARS","description":"Test expiration"}'

# Copiar el ID del response

# 2. Confirmar intent
curl -X POST http://localhost:5000/api/payment-intents/{id}/confirm

# Response esperado:
# {
#   "status": "PendingConfirmation",
#   "confirmedAt": "2025-12-22T...",
#   "expiresAt": "2025-12-22T..." (confirmedAt + 2 min)
# }

# 3. Esperar 2+ minutos → Worker expira automáticamente

# 4. Consultar estado
curl http://localhost:5000/api/payment-intents/{id}

# Response esperado:
# {
#   "status": "Expired",
#   "expiredAt": "2025-12-22T...",
#   "expiresAt": null (limpiado)
# }
```

✅ **Resultado esperado:**
- Intent en `PendingConfirmation` por ~2 minutos
- Worker loguea: "Expired 1 payment intents"
- Intent queda en estado `Expired`
- `ExpiresAt` limpiado (null)

---

### Test 2: Crear → Confirmar → Capturar (antes de expirar)
```bash
# 1. Crear intent
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":10000,"currency":"ARS"}'

# 2. Confirmar
curl -X POST http://localhost:5000/api/payment-intents/{id}/confirm

# 3. Capturar INMEDIATAMENTE (< 2 min)
curl -X POST http://localhost:5000/api/payment-intents/{id}/capture

# Response esperado:
# {
#   "status": "Captured",
#   "capturedAt": "2025-12-22T...",
#   "expiresAt": null (limpiado)
# }

# 4. Esperar 2+ minutos → Worker NO debe expirar este intent
```

✅ **Resultado esperado:**
- Intent capturado exitosamente
- `ExpiresAt` limpiado en el capture
- Worker NO expira intents `Captured`
- Estado final permanece `Captured`

---

### Test 3: Capturar sin confirmar → 409 Conflict
```bash
# 1. Crear intent
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":10000,"currency":"ARS"}'

# 2. Intentar capturar SIN confirmar
curl -X POST http://localhost:5000/api/payment-intents/{id}/capture
```

✅ **Resultado esperado:**
```json
HTTP/1.1 409 Conflict
{
  "error": "Cannot capture payment intent in status Created. Must be in PendingConfirmation status."
}
```

---

### Test 4: Confirmar dos veces → 409 Conflict
```bash
# 1. Crear y confirmar
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":10000,"currency":"ARS"}'

curl -X POST http://localhost:5000/api/payment-intents/{id}/confirm

# 2. Intentar confirmar de nuevo
curl -X POST http://localhost:5000/api/payment-intents/{id}/confirm
```

✅ **Resultado esperado:**
```json
HTTP/1.1 409 Conflict
{
  "error": "Cannot confirm payment intent in status PendingConfirmation. Must be in Created status."
}
```

---

### Test 5: Worker no expira estados finales
```bash
# 1. Crear, confirmar y revertir
curl -X POST http://localhost:5000/api/payment-intents \
  -H "Content-Type: application/json" \
  -d '{"amount":10000,"currency":"ARS"}'

curl -X POST http://localhost:5000/api/payment-intents/{id}/confirm
curl -X POST http://localhost:5000/api/payment-intents/{id}/reverse

# 2. Verificar estado Reversed con ExpiresAt = null
curl http://localhost:5000/api/payment-intents/{id}

# 3. Esperar 2+ minutos → Worker NO debe tocarlo
```

✅ **Resultado esperado:**
- Intent queda en `Reversed`
- `ExpiresAt` limpiado (null)
- Worker NO lo expira
- Permanece `Reversed` indefinidamente

---

## 📊 Verificación de Base de Datos

Conectarse a Postgres:
```bash
docker exec -it payments-postgres psql -U postgres -d payments_db
```

Verificar estructura:
```sql
\d payment_intents

-- Debe mostrar:
-- id, status, amount, currency, description,
-- created_at, updated_at, confirmed_at, expires_at,
-- captured_at, reversed_at, expired_at
```

Verificar índices:
```sql
\di

-- Debe mostrar:
-- ix_payment_intents_status
-- ix_payment_intents_created_at
-- ix_payment_intents_expires_at
```

Consultar datos:
```sql
SELECT id, status, confirmed_at, expires_at, expired_at, captured_at
FROM payment_intents
ORDER BY created_at DESC
LIMIT 10;
```

---

## ✅ Criterios de Aceptación - Fase 1B

### Arquitectura
- ✓ Capa Application creada y desacoplada
- ✓ API y Worker dependen de Application
- ✓ No hay dependencia circular
- ✓ Separación clara de responsabilidades

### Dominio
- ✓ Transiciones de estado correctas
- ✓ Validaciones en el servicio
- ✓ Limpieza de `ExpiresAt` en estados finales
- ✓ HTTP status codes semánticamente correctos

### Persistencia
- ✓ Mapeo EF Core correcto (snake_case)
- ✓ Índices optimizados
- ✓ Migración limpia sin warnings
- ✓ Postgres funcional

### Worker
- ✓ Expira intents automáticamente
- ✓ No afecta estados finales
- ✓ Logging apropiado
- ✓ Manejo de errores

### Testing
- ✓ Flujo completo: Crear → Confirmar → Expirar
- ✓ Flujo alternativo: Crear → Confirmar → Capturar
- ✓ Validaciones: 409 Conflict
- ✓ Worker selectivo (solo PendingConfirmation)

---

## 📝 Entregable Final - Fase 1B

Cuando TODOS los tests pasen:

**✅ FASE 1B CERRADA**

### Lo que funciona:
1. ✅ Persistencia completa en Postgres
2. ✅ Validación de transiciones de estado
3. ✅ Expiración automática funcional
4. ✅ HTTP semantics correctas (409 Conflict)
5. ✅ Arquitectura desacoplada (Application layer)
6. ✅ Limpieza de dominio (ExpiresAt en estados finales)
7. ✅ Worker independiente y funcional

### Próximas fases:
- **Fase 2:** Idempotencia con Redis
- **Fase 3:** Outbox pattern + eventos
- **Fase 4:** Observabilidad (CorrelationId, métricas)
- **Fase 5:** Frontend intercambiable

---

## 🚀 Instrucciones de Ejecución

### 1. Aplicar migración limpia
```bash
cd C:\DesarrolloC#\poc-payments
migrate-phase1b.bat
```

### 2. Ejecutar API
```bash
cd src\Payments.Api
dotnet run
```

### 3. Ejecutar Worker (otra terminal)
```bash
cd src\Payments.Worker
dotnet run
```

### 4. Abrir Swagger
http://localhost:5000/swagger

### 5. Ejecutar tests de validación
Seguir los 5 tests documentados arriba.

---

## ⚠️ Troubleshooting

**Error: "No DbContext found"**
→ Asegúrate de estar en `src/Payments.Api` al ejecutar migrations

**Error: "Cannot connect to database"**
→ Verifica que Docker esté corriendo: `docker ps`

**Worker no expira intents**
→ Verifica que `ExpiresAt` esté seteado al confirmar
→ Chequea los logs del Worker cada 30 segundos

**409 no aparece**
→ Verifica que usaste `Conflict()` en el controller
→ No `BadRequest()`

---

## 📚 Documentos Relacionados

- `docs/fase0-design.md` - Diseño original
- `docs/database-setup.md` - Setup inicial de DB
- `docs/domain-improvements.md` - Mejoras de dominio
- `README.md` - Instrucciones generales
