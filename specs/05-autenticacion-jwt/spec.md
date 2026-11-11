# 05-autenticacion-jwt — Autenticación con JWT

> Repositorio: BusTrack API · Estado: Implementado · Fecha: 2026-10-30.

## 1. Objetivo

Documentar el login JWT HS256 con lockout en SDDSegBloqueo y política de administrador.

## 2. Contexto

Login implementado en POST /api/v1/auth/login. Usa lockout y AdminPolicy.

## 3. Alcance

**Incluye:**

- Validación de credenciales.
- Emisión de JWT.
- Lockout.
- AdminPolicy.

**No incluye:**

- Cambiar el stack objetivo.
- Desplegar recursos o ejecutar migraciones en esta fase.

## 4. Requisitos funcionales

1. Credenciales inválidas no revelan si el usuario existe.
2. Lockout tras intentos fallidos.
3. Token con expiración server-driven.
4. Rate limit en el endpoint.

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
