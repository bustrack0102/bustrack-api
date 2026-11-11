# 12-migracion-de-persistencia-a-cosmos-db — Migración de persistencia a Cosmos DB for NoSQL

> Repositorio: BusTrack API · Estado: Pendiente · Fecha: 2026-10-30.

## 1. Objetivo

Planificar la sustitución de EF Core/SQL Server por Azure Cosmos DB for NoSQL sin ejecutarla.

## 2. Contexto

El código actual usa EF Core + SQL Server con SDDUsuario, SDDSegBloqueo y Testcontainers.MsSql.

## 3. Alcance

**Incluye:**

- Modelo documental.
- Repositorios Cosmos.
- Migración de datos.
- Doble lectura/escritura y rollback.
- Pruebas de equivalencia.

**No incluye:**

- Ejecutar la migración.
- Eliminar SQL Server.
- Cambiar contratos REST.

## 4. Requisitos funcionales

1. Conservar unicidad de usuario/correo.
2. Mantener lockout y blacklist.
3. Documentar mapeo relacional→documental.
4. Rollback probado.

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
