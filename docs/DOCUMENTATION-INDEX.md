# 📚 Índice de Documentación - Payment Intent POC

---

## 🚀 Inicio Rápido

### Para empezar desde cero
1. **[QUICK-START.md](../QUICK-START.md)** - Guía de inicio rápido (5 min)
2. **[README.md](../README.md)** - Documentación general del proyecto

### Scripts de gestión
- `setup.bat` - Setup completo automatizado
- `run-api.bat` - Ejecutar API
- `run-worker.bat` - Ejecutar Worker
- `verify.bat` - Verificar que todo está OK
- `diagnose.bat` - Diagnóstico del sistema

---

## 📖 Documentación por Fase

### Fase 0: Diseño
- **[fase0-design.md](fase0-design.md)** - Diseño inicial, arquitectura, decisiones técnicas

### Fase 1B: Persistencia + Expiración
- **[PHASE1B-COMPLETED.md](PHASE1B-COMPLETED.md)** ⭐ - Resumen completo de lo implementado
- **[fase1b-closure.md](fase1b-closure.md)** - Criterios de validación y cierre
- **[safe-refactor-guide.md](safe-refactor-guide.md)** - Guía completa del refactor arquitectónico
- **[EXECUTIVE-SUMMARY.md](EXECUTIVE-SUMMARY.md)** - Resumen ejecutivo con comandos

---

## 🧪 Documentación de Pruebas

### Casos de prueba completos
- **[test-cases-phase1b.md](test-cases-phase1b.md)** ⭐ - 8 casos de prueba detallados
  - Caso principal: Crear → Confirmar → Expirar
  - Test A: Capturar antes de expirar
  - Test B: Capturar sin confirmar (409)
  - Test C: Confirmar dos veces (409)
  - Test D: Revertir manualmente
  - Test E: Revertir desde Created
  - Test F: Capturar intent expirado (409)
  - Test G: Revertir intent capturado (409)
  - Test H: Múltiples intents expiran simultáneamente

### Guía rápida
- **[quick-test-guide.md](quick-test-guide.md)** - Tests rápidos (5-10 min)

---

## 🛠️ Documentación Técnica

### Base de datos
- **[database-setup.md](database-setup.md)** - Setup inicial de base de datos
- **Comandos útiles:**
  ```bash
  # Ver estructura de tablas
  docker exec -it payments-postgres psql -U postgres -d payments_db
  \dt
  \d payment_intents
  ```

### Arquitectura
- **Estructura del proyecto:**
  ```
  Payments.Shared       (Modelos, DTOs, Enums)
  Payments.Application  (DbContext, Services)
  Payments.Api          (Controllers HTTP)
  Payments.Worker       (Background Services)
  ```

---

## 🔧 Resolución de Problemas

### Guías de troubleshooting
- **[FIX-SETUP-ISSUE.md](../FIX-SETUP-ISSUE.md)** - Solución a problemas con scripts
- **[safe-refactor-guide.md](safe-refactor-guide.md)** - Sección de troubleshooting completa

### Problemas comunes

**1. setup.bat no encuentra el .sln**
- Ejecutar desde la raíz: `cd C:\DesarrolloC#\poc-payments`
- Ejecutar: `diagnose.bat` para ver qué pasa

**2. Tabla payment_intents no se crea**
- Ejecutar: `fix-migrations.bat`
- Verificar con: `docker exec -it payments-postgres psql -U postgres -d payments_db`

**3. Worker no expira intents**
- Verificar que Worker esté corriendo
- Verificar que `expiresAt` se setea al confirmar
- Verificar configuración: `ExpirationTimeoutSeconds: 120`

**4. API no levanta**
- Verificar que puerto 5000 esté libre
- Ver logs en la terminal
- Ejecutar: `verify.bat`

---

## 📊 Comandos Útiles

### Docker
```bash
# Levantar infraestructura
cd infra && docker compose up -d

# Ver logs
docker logs payments-postgres
docker logs payments-redis

# Ver contenedores
docker ps --filter "name=payments"

# Conectarse a Postgres
docker exec -it payments-postgres psql -U postgres -d payments_db
```

### .NET
```bash
# Compilar
dotnet build poc-payments.sln

# Restaurar paquetes
dotnet restore

# Ver referencias
dotnet list src/Payments.Worker/Payments.Worker.csproj reference
```

### EF Migrations
```bash
cd src/Payments.Api

# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Ver migraciones aplicadas
dotnet ef migrations list
```

---

## 📈 Estado del Proyecto

### ✅ Completado (Fase 1B)
- Arquitectura desacoplada (Application layer)
- Persistencia en Postgres con EF Core
- Validación de transiciones de estado
- Expiración automática funcional
- HTTP semantics correctas (409 Conflict)
- Limpieza de dominio (expiresAt)
- Worker independiente del API

### ⏳ Pendiente (Próximas Fases)
- **Fase 2:** Idempotencia con Redis
- **Fase 3:** Outbox pattern + eventos
- **Fase 4:** Observabilidad (CorrelationId, métricas)
- **Fase 5:** Frontend (Next.js dashboard)

---

## 🎯 Flujo de Trabajo Recomendado

### Primera vez (Setup completo)
1. Leer: `README.md`
2. Ejecutar: `setup.bat`
3. Leer: `docs/test-cases-phase1b.md`
4. Probar: Caso principal (Crear → Confirmar → Expirar)
5. Verificar: `verify.bat`

### Desarrollo diario
1. Levantar Docker: `cd infra && docker compose up -d`
2. Ejecutar API: `run-api.bat` (Terminal 1)
3. Ejecutar Worker: `run-worker.bat` (Terminal 2)
4. Abrir Swagger: http://localhost:5000/swagger

### Testing
1. Consultar: `docs/quick-test-guide.md` para tests rápidos
2. Consultar: `docs/test-cases-phase1b.md` para tests completos
3. Verificar en DB con comandos SQL

### Troubleshooting
1. Ejecutar: `diagnose.bat`
2. Ejecutar: `verify.bat`
3. Consultar: `docs/safe-refactor-guide.md` sección troubleshooting
4. Ver logs de Docker

---

## 📞 Recursos Adicionales

### URLs Importantes
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Health Check: http://localhost:5000/api/health

### Estructura de carpetas
```
poc-payments/
├── docs/                      ← Toda la documentación
│   ├── DOCUMENTATION-INDEX.md ← Este archivo
│   ├── PHASE1B-COMPLETED.md   ← Estado actual
│   ├── test-cases-phase1b.md  ← Casos de prueba
│   └── quick-test-guide.md    ← Guía rápida
├── src/
│   ├── Payments.Shared/
│   ├── Payments.Application/  ← DbContext, Services
│   ├── Payments.Api/          ← Controllers
│   └── Payments.Worker/       ← Background Services
├── infra/
│   └── docker-compose.yml
├── *.bat                      ← Scripts de gestión
├── README.md                  ← Documentación general
└── QUICK-START.md             ← Inicio rápido
```

---

## ✅ Checklist de Documentación

### Guías de Setup
- [x] README.md
- [x] QUICK-START.md
- [x] SAFE-REFACTOR-READY.md
- [x] FIX-SETUP-ISSUE.md

### Documentación Técnica
- [x] fase0-design.md
- [x] fase1b-closure.md
- [x] safe-refactor-guide.md
- [x] EXECUTIVE-SUMMARY.md
- [x] database-setup.md

### Documentación de Pruebas
- [x] test-cases-phase1b.md
- [x] quick-test-guide.md

### Documentación de Estado
- [x] PHASE1B-COMPLETED.md
- [x] DOCUMENTATION-INDEX.md

---

## 🎓 Para Nuevos Desarrolladores

### Día 1: Entender el sistema
1. Leer: `README.md`
2. Leer: `docs/fase0-design.md`
3. Leer: `docs/PHASE1B-COMPLETED.md`

### Día 2: Configurar entorno
1. Instalar: Docker Desktop, .NET 8 SDK
2. Clonar repositorio
3. Ejecutar: `setup.bat`
4. Verificar: `verify.bat`

### Día 3: Ejecutar y probar
1. Ejecutar: `run-api.bat` y `run-worker.bat`
2. Probar: `docs/quick-test-guide.md`
3. Explorar: Swagger UI

### Día 4+: Desarrollar
1. Consultar: `docs/test-cases-phase1b.md` antes de cambios
2. Seguir arquitectura establecida
3. Documentar cambios importantes

---

## 🚀 Próximos Pasos

**Fase 1B está completa.** Para continuar:

1. **Revisar:** `docs/PHASE1B-COMPLETED.md` - Sección "Próximas Fases"
2. **Planificar:** Fase 2 - Idempotencia con Redis
3. **Diseñar:** Estructura de Idempotency-Key header
4. **Implementar:** Cache en Redis para keys

---

**Última actualización:** 2025-12-23
**Versión:** Fase 1B
**Estado:** ✅ Completada
