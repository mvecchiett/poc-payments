# 🚀 INICIO RÁPIDO - Fase 1B

## Tu situación actual:
- ✅ Solución compila correctamente
- ✅ Referencias arregladas con dotnet
- ✅ .sln reconstruido con GUIDs correctos
- ⏳ Falta crear las tablas en Postgres

---

## 🎯 PASOS PARA ARRANCAR (EJECUTAR EN ORDEN)

### Paso 1: Levantar infraestructura Docker

```bash
cd C:\DesarrolloC#\poc-payments\infra
docker compose up -d
```

**Verificar que esté corriendo:**
```bash
docker ps --filter "name=payments-postgres"
```

Debe mostrar: `Up X seconds (healthy)`

---

### Paso 2: Crear las tablas con EF Migrations

```bash
cd C:\DesarrolloC#\poc-payments
migrate.bat
```

**Esto hace:**
1. Verifica que Postgres esté corriendo ✓
2. Instala `dotnet-ef` si no existe
3. Crea migración inicial (si no existe)
4. **CREA las tablas en Postgres:**
   - `payment_intents` (tabla principal)
   - `__EFMigrationsHistory` (control de versiones de EF)

**Resultado esperado:**
```
✓ Postgres está corriendo
✓ dotnet-ef disponible
✓ Migración inicial creada
✓ Base de datos actualizada exitosamente

Tablas creadas en Postgres!
```

---

### Paso 3: Ejecutar API

**Terminal 1:**
```bash
cd C:\DesarrolloC#\poc-payments
run-api.bat
```

**Verificar:**
- Debe iniciar en: `http://localhost:5000`
- Abrir Swagger: http://localhost:5000/swagger

**Debe mostrar:**
```
info: Now listening on: http://localhost:5000
info: Application started
```

---

### Paso 4: Ejecutar Worker

**Terminal 2:**
```bash
cd C:\DesarrolloC#\poc-payments
run-worker.bat
```

**Debe mostrar:**
```
info: Expiration Worker Service started
info: Worker running at: ...
```

Se ejecuta cada 30 segundos.

---

## 🧪 PROBAR QUE TODO FUNCIONA

### Test 1: Health check

En el navegador:
```
http://localhost:5000/api/health
```

Debe retornar:
```json
{
  "status": "healthy",
  "timestamp": "..."
}
```

---

### Test 2: Crear intent

En Swagger (http://localhost:5000/swagger):

1. Expandir **POST /api/payment-intents**
2. Click **"Try it out"**
3. Body:
```json
{
  "amount": 10000,
  "currency": "ARS",
  "description": "Test"
}
```
4. Click **"Execute"**

**Resultado esperado:**
- Status: `201 Created`
- Response body con `id`, `status: "Created"`

---

### Test 3: Confirmar intent

1. Copiar el `id` del intent creado
2. Expandir **POST /api/payment-intents/{id}/confirm**
3. Pegar el `id`
4. Click **"Execute"**

**Resultado esperado:**
- Status: `200 OK`
- `status: "PendingConfirmation"`
- `expiresAt`: timestamp (ConfirmedAt + 2 minutos)

---

### Test 4: Expiración automática

1. **Esperar 2+ minutos** (sin hacer nada)
2. El Worker debe loguear en su terminal:
   ```
   info: Payment intent expired: pi_...
   info: Expired 1 payment intents
   ```
3. Consultar el intent:
   - **GET /api/payment-intents/{id}**
4. Verificar:
   - `status: "Expired"`
   - `expiredAt`: timestamp cuando expiró

---

## ✅ CHECKLIST FINAL

Marcar cuando funcione:

- [ ] Docker levantado (Postgres + Redis)
- [ ] `migrate.bat` ejecutado sin errores
- [ ] Tabla `payment_intents` existe en Postgres
- [ ] API levanta en http://localhost:5000
- [ ] Swagger responde en /swagger
- [ ] Health check retorna 200 OK
- [ ] Worker levanta sin errores
- [ ] Worker loguea cada 30 segundos
- [ ] Crear intent funciona (201 Created)
- [ ] Confirmar intent funciona (200 OK, expiresAt seteado)
- [ ] Worker expira intent automáticamente después de 2 min
- [ ] Estado final = Expired

---

## 🐛 SI ALGO FALLA

### migrate.bat falla con "Cannot connect to database"

```bash
# Verificar que Postgres esté corriendo
docker ps --filter "name=payments-postgres"

# Si no está corriendo:
cd infra
docker compose up -d

# Esperar 10 segundos y reintentar
migrate.bat
```

---

### API no levanta

```bash
# Verificar que no haya otro proceso en puerto 5000
netstat -ano | findstr :5000

# Si hay algo, matar el proceso o cambiar el puerto en appsettings.json
```

---

### Worker no expira intents

**Verificar configuración:**
```bash
# Ver src/Payments.Worker/appsettings.json
# Debe tener:
# "ExpirationTimeoutSeconds": 120
```

**Verificar que ExpiresAt se setea al confirmar:**
- Confirmar un intent
- Verificar en el response que `expiresAt` NO es null
- Debe ser: `confirmedAt + 2 minutos`

---

### Verificar que las tablas se crearon

```bash
# Conectarse a Postgres
docker exec -it payments-postgres psql -U postgres -d payments_db

# Ver tablas
\dt

# Debe mostrar:
# payment_intents
# __EFMigrationsHistory

# Ver estructura de payment_intents
\d payment_intents

# Salir
\q
```

---

## 🔍 COMANDOS DE DIAGNÓSTICO

### Ver datos en la tabla

```bash
docker exec -it payments-postgres psql -U postgres -d payments_db -c "SELECT id, status, confirmed_at, expires_at, expired_at FROM payment_intents ORDER BY created_at DESC LIMIT 5;"
```

---

### Ver logs de los contenedores

```bash
docker logs payments-postgres
docker logs payments-redis
```

---

### Verificar que el Worker está conectado a la DB

En los logs del Worker debe aparecer:
```
info: Worker running at: ...
```

Sin errores de conexión.

---

## 📊 ARQUITECTURA DE MIGRACIONES

### ¿Quién crea las tablas?

**Entity Framework Core Migrations:**

1. **Diseñas el modelo** en código:
   ```csharp
   public class PaymentIntent
   {
       public string Id { get; set; }
       public PaymentIntentStatus Status { get; set; }
       // ... otros campos
   }
   ```

2. **EF genera el script SQL** automáticamente:
   ```bash
   dotnet ef migrations add InitialCreate
   ```
   Esto crea archivos en `src/Payments.Api/Migrations/`

3. **EF aplica el script a la DB**:
   ```bash
   dotnet ef database update
   ```
   Esto ejecuta el SQL en Postgres y crea las tablas.

### Flujo completo:

```
Modelo C# → dotnet ef migrations add → Archivos de migración
                                              ↓
                           dotnet ef database update
                                              ↓
                                       Tablas en Postgres
```

---

## ✅ TODO LISTO

Cuando TODOS los checks estén OK:

**🎉 FASE 1B COMPLETADA**

Tienes:
- ✅ Arquitectura desacoplada (Application layer)
- ✅ Persistencia en Postgres
- ✅ Validación de transiciones
- ✅ Expiración automática funcional
- ✅ HTTP 409 Conflict
- ✅ Worker independiente

**Listo para Fase 2: Idempotencia con Redis** 🚀

---

## 📞 SIGUIENTE PASO INMEDIATO

**EJECUTAR AHORA:**

```bash
# 1. Levantar Docker (si no está)
cd C:\DesarrolloC#\poc-payments\infra
docker compose up -d

# 2. Crear las tablas
cd ..
migrate.bat

# 3. Ejecutar API (Terminal 1)
run-api.bat

# 4. Ejecutar Worker (Terminal 2)
run-worker.bat

# 5. Probar en Swagger
# http://localhost:5000/swagger
```

**¡Y a testear!** 🎯
