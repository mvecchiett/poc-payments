# ⚠️ SOLUCIÓN RÁPIDA - Problema con carácter # en la ruta

## 🔍 Problema identificado

Tu ruta tiene un carácter especial: `C:\DesarrolloC#\poc-payments`

El carácter `#` puede causar problemas con scripts batch antiguos.

## ✅ SOLUCIÓN: Scripts actualizados

He actualizado TODOS los scripts para manejar correctamente rutas con caracteres especiales:

- ✅ `setup.bat` - Ahora busca el .sln dinámicamente
- ✅ `build.bat` - Busca automáticamente el archivo .sln
- ✅ `verify.bat` - Usa delayed expansion para variables
- ✅ `diagnose.bat` - **NUEVO** - Diagnóstico completo del sistema

## 🚀 EJECUTA ESTO AHORA (EN ORDEN)

### 1. Diagnóstico completo
```bash
cd C:\DesarrolloC#\poc-payments
diagnose.bat
```

**Esto te mostrará:**
- ✓ Si encuentra el .sln
- ✓ Si todos los proyectos existen
- ✓ Si Docker está corriendo
- ✓ Estado de los contenedores
- ✓ Si dotnet-ef está instalado

---

### 2. Levantar Docker (si no está)
```bash
cd infra
docker compose up -d
cd ..
```

**Verificar:**
```bash
docker ps --filter "name=payments"
```

Debe mostrar:
```
payments-postgres   Up ... (healthy)
payments-redis      Up ... (healthy)
```

---

### 3. Setup completo (ARREGLADO)
```bash
setup.bat
```

**Ahora debe funcionar correctamente:**
```
✓ Solución encontrada: poc-payments.sln
✓ Payments.Application ya existe en la solución
✓ Paquetes restaurados
✓ Solución compilada exitosamente
✓ Postgres está corriendo
```

---

### 4. Crear las tablas
```bash
migrate.bat
```

**Debe mostrar:**
```
✓ Postgres está corriendo
✓ dotnet-ef disponible
✓ Migración inicial creada (o ya existe)
✓ Base de datos actualizada exitosamente

Tablas creadas en Postgres!
```

---

### 5. Verificar todo está OK
```bash
verify.bat
```

**Resultado esperado:**
```
✅ TODAS LAS VERIFICACIONES OK

Sistema listo para ejecutar:
  1. run-api.bat
  2. run-worker.bat
  3. http://localhost:5000/swagger
```

---

### 6. Ejecutar API y Worker

**Terminal 1:**
```bash
run-api.bat
```

**Terminal 2:**
```bash
run-worker.bat
```

---

## 🔍 Si setup.bat TODAVÍA falla

Ejecuta el diagnóstico primero:
```bash
diagnose.bat
```

Y pégame la salida completa.

---

## 🎯 Flujo completo correcto

```bash
# 1. Diagnóstico
diagnose.bat

# 2. Docker
cd infra
docker compose up -d
cd ..

# 3. Compilar (sin migración aún)
build.bat

# 4. Crear tablas
migrate.bat

# 5. Verificar
verify.bat

# 6. Ejecutar
run-api.bat     # Terminal 1
run-worker.bat  # Terminal 2

# 7. Probar
# http://localhost:5000/swagger
```

---

## ✅ Cambios aplicados en los scripts

### setup.bat
- ✅ Usa `setlocal enabledelayedexpansion`
- ✅ Busca el .sln con `FOR %%F IN (*.sln)`
- ✅ Usa variables con `!VARIABLE!` en lugar de `%VARIABLE%`

### build.bat
- ✅ Mismo patrón que setup.bat
- ✅ Busca dinámicamente el archivo .sln

### verify.bat
- ✅ Variables con delayed expansion
- ✅ Más robusto con errores

### diagnose.bat (NUEVO)
- ✅ Muestra TODO el estado del sistema
- ✅ Ayuda a identificar problemas

---

## 📊 ¿Qué hace cada script?

| Script | Función |
|--------|---------|
| `diagnose.bat` | Diagnóstico completo (EMPEZAR AQUÍ) |
| `setup.bat` | Setup completo (limpieza + build + migración) |
| `build.bat` | Solo compilar la solución |
| `migrate.bat` | Solo crear/actualizar tablas en DB |
| `verify.bat` | Verificar que todo está listo |
| `run-api.bat` | Ejecutar API |
| `run-worker.bat` | Ejecutar Worker |
| `reset-db.bat` | DESTRUCTIVO - Resetear DB desde cero |

---

## 🆘 Si nada funciona

Ejecuta en orden:

```bash
# 1. Diagnóstico
diagnose.bat

# 2. Ver qué hay en el directorio
dir

# 3. Ver si el .sln está ahí
dir *.sln

# 4. Intentar compilar directamente
dotnet build poc-payments.sln
```

Y pégame las salidas de cada comando.

---

## ✅ EJECUTA AHORA

```bash
cd C:\DesarrolloC#\poc-payments
diagnose.bat
```

Y dime qué muestra. Eso me dirá exactamente qué está pasando.
