# Tareas — 02-arquitectura-base-y-pipeline-http — Arquitectura base y pipeline HTTP

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado.
> Marcar solo con evidencia. No marcar por documentar.

## Dependencias

1. T01 antes de T02
2. T02 antes de T03

## Actividades

### T01. Consolidar middleware

- Excepciones.
- Correlación.
- Headers.

**Archivos:** API_SDD/Middleware/, API_SDD/Hosting/, API_SDD/Health/, API_SDD/Common/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

### T02. Configurar hosting

- Health checks.
- Probes.
- Rate limiting.
- HSTS/CORS.

**Archivos:** API_SDD/Middleware/, API_SDD/Hosting/, API_SDD/Health/, API_SDD/Common/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

### T03. Probar pipeline

- Errores.
- Headers.
- Límites.

**Archivos:** API_SDD/Middleware/, API_SDD/Hosting/, API_SDD/Health/, API_SDD/Common/

**Terminado cuando:** existe evidencia verificable en código, prueba o documento.

## Cierre

- [ ] Todas las tareas con evidencia.
- [ ] `Memoria.md` actualizado.
- [ ] Pruebas y gates ejecutados.
- [ ] Sin afirmaciones de implementación no verificadas.
