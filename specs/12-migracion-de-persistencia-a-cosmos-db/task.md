# Tareas — 12-migracion-de-persistencia-a-cosmos-db — Conexión a Azure DocumentDB (BusTrackDB)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Completado · Fecha: 2026-11-23.

## Dependencias

1. T01 antes de T02
2. T02 antes de T03
3. T03 antes de T04

## Actividades

### T01. Adaptar MongoDbContext para Azure DocumentDB y BusTrackDB
- Soporte dual para `configuration["MongoDb:ConnectionString"]` y `configuration.GetConnectionString("MongoDb")`.
- Fallback por defecto a base de datos `BusTrackDB`.
- Validación de cadena limpia mediante `ConfigurationSanitizer`.

**Archivos:** `API_SDD/Data/MongoDbContext.cs`  
**Terminado cuando:** Código actualizado y compilación limpia.

### T02. Actualizar configuraciones de entornos y despliegue
- Actualizar `API_SDD/appsettings.Production.json` con `"Database": "BusTrackDB"`.
- Actualizar pipeline `.github/workflows/cd-azure.yml` con `MongoDb__Database=BusTrackDB`.
- Actualizar variables e infraestructura en `infra/terraform/`.

**Archivos:** `API_SDD/appsettings.Production.json`, `.github/workflows/cd-azure.yml`, `infra/terraform/`  
**Terminado cuando:** Referencias a base de datos alineadas a `BusTrackDB`.

### T03. Actualizar documentación SDD y diagramas de migración
- Actualizar `spec.md`, `plan.md`, `task.md` y `chart.md` con los detalles de Azure DocumentDB `bustrack-db` y 34 colecciones migradas.
- Mantener bloque de aprobación para cumplimiento de `check_sdd.ps1`.

**Archivos:** `specs/12-migracion-de-persistencia-a-cosmos-db/`  
**Terminado cuando:** `check_sdd.ps1` retorna `SDD-OK`.

### T04. Verificación de seguridad, build y actualización de Memoria.md
- Registrar migración en `Memoria.md` cumpliendo con sección 8 de lineamientos.
- Validar `dotnet build -c Release` (0 errores, 0 warnings).
- Validar scripts de guardrails y checklist de PR.

**Archivos:** `Memoria.md`, `specs/12-migracion-de-persistencia-a-cosmos-db/checklists/pr.md`  
**Terminado cuando:** Build y guardrails en verde, memoria actualizada.
