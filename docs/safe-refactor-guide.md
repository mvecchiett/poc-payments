# Safe Refactor - Fase 1B
## Desacoplamiento Worker-API sin romper el build

---

## 📋 Resumen de cambios

### Estructura ANTES:
```
Payments.Shared     (Modelos)
Payments.Api        (API + DbContext + Services)  ← Todo acoplado
Payments.Worker     (Worker → depende de API)     ← Acoplamiento indeseado
```

### Estructura DESPUÉS:
```
Payments.Shared       (Modelos, DTOs, Enums)
Payments.Application  (DbContext, Services)       ← Nueva capa compartida
Payments.Api          (Controllers HTTP)          ← Solo HTTP
Payments.Worker       (Background)                ← Solo Background
```

---

## 🎯 Objetivos del refactor

1. ✅ Desacoplar Worker del API
2. ✅ Eliminar duplicación de código (DbContext, Services)
3. ✅ Mantener build estable en todo momento
4. ✅ No editar .sln manualmente
5. ✅ Scripts de migración seguros (no destructivos por defecto)

---

## 📁 Archivos creados/modificados

### ✅ Archivos CREADOS:

**Payments.Application (nuevo proyecto):**
- `src/Payments.Application/Payments.Application.csproj`
- `src/Payments.Application/Data/PaymentsDbContext.cs`
- `src/Payments.Application/Services/IPaymentIntentService.cs`
- `src/Payments.Application/Services/PaymentIntentService.cs`

**Scripts de gestión:**
- `cleanup-duplicates.bat` - Elimina carpetas duplicadas
- `setup.bat` - Setup completo automatizado
- `build.bat` - Compilación de la solución
- `migrate.bat` - Migración NO destructiva (recomendado)
- `reset-db.bat` - Migración destructiva (solo dev/testing)
- `run-api.bat` - Ejecutar API
- `run-worker.bat` - Ejecutar Worker

### ✅ Archivos MODIFICADOS:

**Payments.Api:**
- `src/Payments.Api/Payments.Api.csproj` - Agregada referencia a Application
- `src/Payments.Api/Program.cs` - Usa servicios de Application
- `src/Payments.Api/Controllers/PaymentIntentsController.cs` - Importa de Application

**Payments.Worker:**
- `src/Payments.Worker/Payments.Worker.csproj` - Referencia a Application (NO a API)
- `src/Payments.Worker/Program.cs` - Usa servicios de Application
- `src/Payments.Worker/Services/ExpirationWorkerService.cs` - Importa de Application

### ❌ Archivos ELIMINADOS:
- `src/Payments.Api/Data/` (carpeta completa - ahora en Application)
- `src/Payments.Api/Services/` (carpeta completa - ahora en Application)

---

## 🚀 Comandos de setup (EJECUTAR EN ORDEN)

### Opción A: Setup automático (recomendado)

```bash
# Desde la raíz del proyecto
cd C:\DesarrolloC#\poc-payments

# 1. Setup completo
setup.bat

# Esto ejecuta automáticamente:
# - Limpieza de duplicados
# - dotnet sln add Payments.Application
# - dotnet restore
# - dotnet build
# - migrate.bat
```

### Opción B: Setup manual (paso a paso)

```bash
# Desde la raíz del proyecto
cd C:\DesarrolloC#\poc-payments

# 1. Limpiar duplicados en API
cleanup-duplicates.bat

# 2. Agregar Application a la solución (NO editar .sln a mano)
dotnet sln poc-payments.sln add src\Payments.Application\Payments.Application.csproj

# 3. Verificar que se agregó correctamente
dotnet sln poc-payments.sln list

# Debe mostrar:
# Payments.Api
# Payments.Worker
# Payments.Shared
# Payments.Application  ← NUEVO

# 4. Restaurar paquetes
dotnet restore

# 5. Compilar la solución
dotnet build poc-payments.sln

# Debe compilar sin errores

# 6. Aplicar migraciones (NO destructivo)
migrate.bat
```

---

## 🛠️ Comandos de build y ejecución

### Compilar la solución completa:
```bash
# Opción 1: Script
build.bat

# Opción 2: Manual
dotnet clean
dotnet restore
dotnet build poc-payments.sln
```

### Ejecutar API:
```bash
# Opción 1: Script
run-api.bat

# Opción 2: Manual
cd src\Payments.Api
dotnet run

# URL: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Ejecutar Worker (otra terminal):
```bash
# Opción 1: Script
run-worker.bat

# Opción 2: Manual
cd src\Payments.Worker
dotnet run
```

---

## 🗄️ Migraciones de base de datos

### migrate.bat (NO destructivo - recomendado)
```bash
migrate.bat

# Lo que hace:
# 1. Verifica que Postgres esté corriendo
# 2. Crea migración si no existe
# 3. Aplica migraciones pendientes
# 4. NO elimina datos existentes
```

### reset-db.bat (DESTRUCTIVO - solo dev/testing)
```bash
reset-db.bat

# ⚠️ ADVERTENCIA: Elimina TODOS los datos
# Lo que hace:
# 1. DROP DATABASE (elimina todo)
# 2. Elimina carpeta Migrations
# 3. Crea migración inicial desde cero
# 4. Aplica migración
```

### Crear nueva migración (cuando cambies el modelo):
```bash
cd src\Payments.Api

dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update
```

**IMPORTANTE para EF Migrations:**
- Ejecutar siempre desde `src/Payments.Api` (startup project)
- El DbContext vive en `Payments.Application` pero EF lo descubre automáticamente
- Usar `--project` y `--startup-project` si hay problemas:

```bash
dotnet ef migrations add MigracionNueva \
  --project src/Payments.Application \
  --startup-project src/Payments.Api
```

---

## ✅ Checklist de validación

### 1. Compilación
```bash
dotnet build poc-payments.sln
```
✅ Debe compilar sin errores ni warnings

### 2. Verificar dependencias
```bash
dotnet list src/Payments.Api/Payments.Api.csproj reference
```
✅ Debe mostrar: Payments.Application, Payments.Shared

```bash
dotnet list src/Payments.Worker/Payments.Worker.csproj reference
```
✅ Debe mostrar: Payments.Application, Payments.Shared
❌ NO debe mostrar: Payments.Api

### 3. API funciona
```bash
run-api.bat
```
✅ Levanta en http://localhost:5000
✅ Swagger responde en /swagger
✅ Health check: GET /api/health → 200 OK

### 4. Worker funciona
```bash
run-worker.bat
```
✅ Levanta sin errores
✅ Loguea cada 30 segundos
✅ Expira intents en PendingConfirmation

### 5. No hay duplicados
```bash
dir src\Payments.Api\Data        # ❌ No debe existir
dir src\Payments.Api\Services    # ❌ No debe existir
dir src\Payments.Application\Data        # ✅ Debe existir
dir src\Payments.Application\Services    # ✅ Debe existir
```

### 6. Flujo completo funciona
1. Crear intent → 201 Created
2. Confirmar intent → 200 OK (expiresAt seteado)
3. Esperar 2+ minutos → Worker expira automáticamente
4. Consultar intent → Status = Expired

---

## 🔍 Troubleshooting

### Error: "Project not found in solution"
```bash
# Agregar manualmente
dotnet sln poc-payments.sln add src\Payments.Application\Payments.Application.csproj
```

### Error: "Type or namespace 'Application' could not be found"
```bash
# Verificar referencia en .csproj
dotnet list src/Payments.Api/Payments.Api.csproj reference

# Debe incluir Payments.Application
# Si no está, agregar:
dotnet add src/Payments.Api/Payments.Api.csproj reference src/Payments.Application/Payments.Application.csproj
```

### Error: "No DbContext was found"
```bash
# Asegurarse de ejecutar desde src/Payments.Api
cd src/Payments.Api
dotnet ef migrations add TestMigration
```

### Error: "Database 'payments_db' already exists"
```bash
# Si quieres resetear (DESTRUCTIVO):
reset-db.bat

# Si quieres aplicar solo cambios (NO destructivo):
migrate.bat
```

### Worker no expira intents
```bash
# Verificar que el Worker esté corriendo
# Verificar configuración en appsettings.json:
# "ExpirationTimeoutSeconds": 120
# Verificar que ExpiresAt esté seteado al confirmar
```

---

## 📊 Verificación de arquitectura

### Comando para visualizar referencias:
```bash
dotnet list package --include-transitive
```

### Dependencias esperadas:

**Payments.Api:**
- → Payments.Application ✅
- → Payments.Shared ✅

**Payments.Worker:**
- → Payments.Application ✅
- → Payments.Shared ✅

**Payments.Application:**
- → Payments.Shared ✅
- → Npgsql.EntityFrameworkCore.PostgreSQL ✅

### Verificar que NO exista:
- Payments.Worker → Payments.Api ❌

---

## 📝 Comandos de desarrollo

### Ver lista de proyectos en la solución:
```bash
dotnet sln poc-payments.sln list
```

### Agregar un proyecto a la solución:
```bash
dotnet sln poc-payments.sln add <ruta-del-csproj>
```

### Remover un proyecto de la solución:
```bash
dotnet sln poc-payments.sln remove <ruta-del-csproj>
```

### Agregar referencia entre proyectos:
```bash
dotnet add <proyecto-origen> reference <proyecto-destino>
```

### Ver referencias de un proyecto:
```bash
dotnet list <proyecto> reference
```

### Limpiar build:
```bash
dotnet clean
```

### Restaurar paquetes:
```bash
dotnet restore
```

### Compilar:
```bash
dotnet build [--no-restore]
```

---

## ✅ Criterios de éxito - Safe Refactor

Al finalizar el refactor, TODOS estos deben estar OK:

### Arquitectura:
- ✅ Payments.Application existe y compila
- ✅ API depende de Application
- ✅ Worker depende de Application
- ✅ Worker NO depende de API
- ✅ No hay carpetas duplicadas (Data/Services en API)

### Build:
- ✅ `dotnet build` compila sin errores
- ✅ `dotnet sln list` muestra los 4 proyectos
- ✅ No hay warnings de referencias circulares

### Funcionalidad:
- ✅ API levanta y Swagger responde
- ✅ Worker levanta y expira intents
- ✅ Migraciones funcionan desde src/Payments.Api
- ✅ Crear → Confirmar → Expirar funciona correctamente

### Migraciones:
- ✅ `migrate.bat` funciona (NO destructivo)
- ✅ `reset-db.bat` disponible (claramente marcado como destructivo)
- ✅ `dotnet ef database update` funciona desde src/Payments.Api

---

## 🎯 Próximos pasos después del refactor

Una vez validado que todo funciona:

1. **Fase 2**: Idempotencia con Redis
2. **Fase 3**: Outbox pattern + eventos
3. **Fase 4**: Observabilidad (CorrelationId, métricas)
4. **Fase 5**: Frontend intercambiable

---

## 📚 Referencias

- `docs/fase0-design.md` - Diseño original
- `docs/fase1b-closure.md` - Validación Fase 1B
- `README.md` - Documentación general
