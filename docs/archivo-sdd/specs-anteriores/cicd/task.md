# Tareas — CI/CD y gates de calidad

> Spec: `spec.md` · Plan: `plan.md` · Estado: Parcial.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias entre tareas

1. T01 antes de T02
2. T02 antes de T03

## Tareas

### T01. Auditar workflow actual

- Pasos.
- Gates reales.

**Archivos:** .github/workflows/

**Terminado cuando:** Mapa de gates actuales.

### T02. Ajustar pipeline

- Quitar `|| true`.
- Añadir SAST/SCA.
- Cobertura.

**Archivos:** .github/workflows/

**Terminado cuando:** Gates bloqueantes configurados.

### T03. Documentar evidencias

- Artefactos.
- Umbrales.

**Archivos:** specs/cicd/spec.md

**Terminado cuando:** Criterios de verde documentados.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
