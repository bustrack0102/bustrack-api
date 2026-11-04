# Tareas — Migración SQL Server/EF Core → Azure Cosmos DB for NoSQL

> Spec: `spec.md` · Plan: `plan.md` · Estado: Pendiente.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias entre tareas

1. T01 antes de T02
2. T02 antes de T03
3. T03 antes de T04

## Tareas

### T01. Diseñar modelo NoSQL

- Agregados.
- Particiones.
- Índices.

**Archivos:** specs/migraciones/cosmos-db/plan.md

**Terminado cuando:** Modelo documental revisado.

### T02. Diseñar repositorios

- Interfaces.
- Implementación Cosmos.
- ETag.

**Archivos:** plan.md

**Terminado cuando:** Contratos y concurrencia definidos.

### T03. Plan de migración y rollback

- Fases.
- Datos.
- Validación.

**Archivos:** task.md

**Terminado cuando:** Plan reversible documentado.

### T04. Pruebas de equivalencia

- Contrato.
- Datos.
- Concurrencia.

**Archivos:** task.md

**Terminado cuando:** Criterios de aceptación definidos.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
