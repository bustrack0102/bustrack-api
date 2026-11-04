# 06-gestion-de-sesiones-y-logout — Gestión de sesiones y logout

> Repositorio: BusTrack API · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Documentar el logout con revocación en blacklist sobre IMemoryCache.

## 2. Contexto

Logout implementado en POST /api/v1/auth/logout.

## 3. Alcance

**Incluye:**

- Revocación de token.
- Blacklist en memoria.
- Idempotencia.

**No incluye:**

- Cambiar el stack objetivo.
- Desplegar recursos o ejecutar migraciones en esta fase.

## 4. Requisitos funcionales

1. Un token revocado no puede reutilizarse.
2. Logout es idempotente.
3. No se registran tokens.
4. La blacklist expira según el token.

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
