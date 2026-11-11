# 02-arquitectura-base-y-pipeline-http — Arquitectura base y pipeline HTTP

> Repositorio: BusTrack API · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Establecer el pipeline HTTP común: middleware, headers, correlación, excepciones, health checks, probes, rate limiting, HSTS y CORS.

## 2. Contexto

Implementado en API_SDD/{Middleware,Hosting,Health,Common}.

## 3. Alcance

**Incluye:**

- Middleware de excepciones y correlación.
- Headers de seguridad.
- Health checks y probes.
- Rate limiting.
- HSTS y CORS.

**No incluye:**

- Cambiar el stack objetivo.
- Desplegar recursos o ejecutar migraciones en esta fase.

## 4. Requisitos funcionales

1. Toda respuesta incluye correlación.
2. Los errores usan un contrato uniforme.
3. Health y probes distinguen liveness/readiness.
4. Rate limit responde 429 con Retry-After.

## 5. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad tarea -> prueba -> evidencia.

## 6. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 7. Criterios de aceptación

- [ ] La tarea tiene criterios verificables y evidencia asociada.
- [ ] No se declara implementado lo que no existe en código o pruebas.
- [ ] Los contratos consumidos o expuestos quedan documentados.
- [ ] `Memoria.md` refleja el estado resultante.

## 8. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`

## 9. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.

## Aprobación

- [x] Revisión de arquitectura y contrato completada — fecha: 2026-11-11
- [x] CHECKLIST_PR.md validado
- [x] Sin conflicto con constitution.md
