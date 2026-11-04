# Migración SQL Server/EF Core → Azure Cosmos DB for NoSQL

> Repositorio: BusTrack API · Ruta: `specs/migraciones/cosmos-db/` · Estado: Pendiente · Fecha: 2026-10-30.

## 1. Identificador y nombre

- **ID:** `migraciones/cosmos-db`
- **Nombre:** Migración SQL Server/EF Core → Azure Cosmos DB for NoSQL
- **Repositorio:** BusTrack API

## 2. Objetivo

Documentar la migración de persistencia a Cosmos DB for NoSQL sin ejecutarla.

## 3. Contexto

El código actual usa EF Core + SQL Server con SDDUsuario, SDDSegBloqueo, migraciones y Testcontainers.MsSql.

## 4. Alcance

**Incluye:**

- Modelo NoSQL por agregados.
- Repositorios sobre SDK Cosmos.
- Estrategia de migración de datos.
- Equivalencia funcional y pruebas.

**No incluye:**

- Ejecutar la migración ahora.
- Cambiar contratos públicos.
- Eliminar el código SQL Server.

## 5. Requisitos funcionales

1. Mantener el contrato REST durante la migración.
2. Preservar unicidad de usuario/correo y concurrencia.
3. Mantener lockout y blacklist.
4. Documentar mapeo relacional→documental.

## 6. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad spec -> tarea -> prueba.

## 7. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 8. Criterios de aceptación

- [ ] Migración por fases y con doble lectura/escritura si aplica.
- [ ] Pruebas de equivalencia contra ambos almacenes.
- [ ] Rollback documentado.
- [ ] Sin downtime no planificado.

## 9. Restricciones

- No ejecutar migraciones reales en esta fase.
- No borrar SQL Server hasta validar.

## 10. Dependencias

- `docs/contratos-integracion.md`
- specs/02-arquitectura/

## 11. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.
