# Tareas — Estrategia de pruebas

> Spec: `spec.md` · Plan: `plan.md` · Estado: Parcial.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias entre tareas

1. T01 antes de T02
2. T02 antes de T03

## Tareas

### T01. Inventariar suites

- Qué cubre cada una.
- Huecos.

**Archivos:** specs/pruebas/spec.md

**Terminado cuando:** Mapa de cobertura funcional.

### T02. Definir datos y fixtures

- Sin secretos.
- Deterministas.

**Archivos:** pruebas

**Terminado cuando:** Fixtures documentados.

### T03. Integrar en CI

- Comando único.
- Cobertura.
- Bloqueante.

**Archivos:** .github/workflows/

**Terminado cuando:** CI ejecuta y reporta pruebas.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
