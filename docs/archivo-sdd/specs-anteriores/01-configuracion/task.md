# Tareas — 01 — Configuración del entorno

> Spec: `spec.md` · Plan: `plan.md` · Estado: Parcial.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias entre tareas

1. T01 antes de T02
2. T02 antes de T03

## Tareas

### T01. Inventariar configuración

- Listar claves usadas.
- Detectar claves no documentadas.

**Archivos:** código + configuración

**Terminado cuando:** Inventario completo en docs.

### T02. Documentar entornos

- Dev, test y prod.
- Secretos y placeholders.

**Archivos:** docs/configuracion.md

**Terminado cuando:** Variables y origen documentados.

### T03. Alinear CI

- Secretos de GitHub.
- Valores no productivos.

**Archivos:** .github/workflows/

**Terminado cuando:** CI no depende de secretos reales.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
