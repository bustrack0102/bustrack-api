# 02 — Arquitectura y límites del servicio

> Repositorio: BusTrack API · Ruta: `specs/02-arquitectura/` · Estado: Parcial · Fecha: 2026-10-30.

## 1. Identificador y nombre

- **ID:** `02-arquitectura`
- **Nombre:** 02 — Arquitectura y límites del servicio
- **Repositorio:** BusTrack API

## 2. Objetivo

Definir la arquitectura interna de BusTrack API y sus límites con BusTrack.

## 3. Contexto

Arquitectura actual: API_SDD/{Common,Controllers/V1,Data,Dtos,Health,Hosting,Logging,Middleware,OpenApi,Security,Services,Validators}; pruebas en UnitTest, IntegrationTest, SecurityTest, ContractTest y DatabaseTest

## 4. Alcance

**Incluye:**

- Componentes y responsabilidades.
- Límites con API, clientes, ML/GenAI y Cosmos.
- Dependencias permitidas.
- Estrategia de evolución.

**No incluye:**

- Implementar features de dominio.
- Desplegar.
- Cambiar contratos públicos sin spec.

## 5. Requisitos funcionales

1. Mantener separación de capas y responsabilidades.
2. No acceder a Cosmos desde clientes.
3. No duplicar la fuente autorizada de datos.
4. Documentar componentes y flujos.

## 6. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad spec -> tarea -> prueba.

## 7. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 8. Criterios de aceptación

- [ ] El diagrama coincide con `chart.md`.
- [ ] Cada componente tiene responsabilidad única.
- [ ] No hay dependencias prohibidas.
- [ ] La transición está documentada.

## 9. Restricciones

- No ejecutar migraciones ni despliegues en esta fase.
- No cambiar el stack objetivo sin autorización.
- No incluir secretos ni PII.

## 10. Dependencias

- `constitution.md`
- `docs/contratos-integracion.md`

## 11. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.
