# Configuración de Base de Datos - Fase 1B

## Prerequisitos

1. **Docker** con Postgres corriendo:
   ```bash
   cd infra
   docker compose up -d
   ```

2. **dotnet-ef tool** instalado globalmente:
   ```bash
   dotnet tool install --global dotnet-ef
   ```
   
   O actualizar si ya lo tienes:
   ```bash
   dotnet tool update --global dotnet-ef
   ```

## Opción 1: Script automatizado (Windows)

```bash
cd C:\DesarrolloC#\poc-payments
setup-database.bat
```

Este script:
1. Restaura paquetes NuGet
2. Crea la migración inicial
3. Aplica la migración a Postgres

## Opción 2: Comandos manuales

```bash
cd src\Payments.Api

# Restaurar paquetes
dotnet restore

# Crear migración
dotnet ef migrations add InitialCreate

# Aplicar migración
dotnet ef database update
```

## Verificar la tabla creada

Puedes conectarte a Postgres y verificar:

```bash
docker exec -it payments-postgres psql -U postgres -d payments_db

# Dentro de psql:
\dt                          # Listar tablas
\d payment_intents           # Describir tabla
SELECT * FROM payment_intents;  # Consultar datos
\q                          # Salir
```

## Estructura de la tabla

```sql
CREATE TABLE payment_intents (
    id VARCHAR(100) PRIMARY KEY,
    status VARCHAR NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    confirmed_at TIMESTAMP,
    captured_at TIMESTAMP,
    reversed_at TIMESTAMP,
    expired_at TIMESTAMP
);

-- Índices
CREATE INDEX ix_payment_intents_status ON payment_intents(status);
CREATE INDEX ix_payment_intents_created_at ON payment_intents(created_at);
```

## Troubleshooting

### Error: "dotnet-ef command not found"
```bash
dotnet tool install --global dotnet-ef
```

### Error: "No DbContext was found"
Asegúrate de estar en la carpeta `src/Payments.Api` cuando ejecutes los comandos.

### Error: "Cannot connect to database"
Verifica que Postgres esté corriendo:
```bash
docker ps --filter "name=payments-postgres"
```

### Resetear la base de datos
```bash
# Eliminar migraciones
dotnet ef database drop --force

# Recrear
dotnet ef database update
```
