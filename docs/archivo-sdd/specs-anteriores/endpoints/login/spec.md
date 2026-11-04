# Endpoint / flujo de login

> Repositorio: BusTrack API · Ruta: `specs/endpoints/login/` · Estado: Implementado · Fecha: 2026-10-30.

## 1. Identificador y nombre

- **ID:** `endpoints/login`
- **Nombre:** Endpoint / flujo de login
- **Repositorio:** BusTrack API

## 2. Objetivo

Documentar y garantizar el flujo de inicio de sesión en API central.

## 3. Contexto

JWT HS256 en POST /api/v1/auth/login y POST /api/v1/auth/logout (blacklist en IMemoryCache).

## 4. Alcance

**Incluye:**

- Contrato de entrada/salida.
- Validaciones y errores.
- Sesión/token y expiración.
- Rate limiting y auditoría.

**No incluye:**

- Registro de usuarios nuevos.
- Recuperación de contraseña.
- SSO o biometría.

## 5. Requisitos funcionales

1. Aceptar credenciales y devolver token/sesión según el repositorio.
2. Mapear errores sin filtrar información sensible.
3. Respetar expiración server-driven.
4. Registrar eventos sin PII.

## 6. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad spec -> tarea -> prueba.

## 7. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 8. Criterios de aceptación

- [ ] Login exitoso devuelve el contrato esperado.
- [ ] Credenciales inválidas no revelan si el usuario existe.
- [ ] Rate limit responde 429 con Retry-After.
- [ ] Logs sin token ni contraseña.

## 9. Restricciones

- No ejecutar migraciones ni despliegues en esta fase.
- No cambiar el stack objetivo sin autorización.
- No incluir secretos ni PII.

## 10. Dependencias

- API central
- `docs/contratos-integracion.md`

## 11. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.
