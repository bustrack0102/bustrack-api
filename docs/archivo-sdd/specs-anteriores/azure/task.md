# Tareas — Arquitectura Azure propuesta

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesta.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias entre tareas

1. T01 antes de T02
2. T02 antes de T03

## Tareas

### T01. Definir topología

- Recursos.
- Responsabilidades.
- Conectividad.

**Archivos:** specs/azure/plan.md

**Terminado cuando:** Diagrama y tabla de recursos.

### T02. Definir configuración/secretos

- Variables.
- Key Vault.

**Archivos:** specs/azure/spec.md

**Terminado cuando:** Variables y secretos documentados sin valores.

### T03. Definir operación

- Monitoreo.
- Escalabilidad.
- Costos.

**Archivos:** specs/azure/chart.md

**Terminado cuando:** Operación y límites documentados.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
