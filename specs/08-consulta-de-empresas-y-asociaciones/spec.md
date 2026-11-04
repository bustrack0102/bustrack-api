# 08-consulta-de-empresas-y-asociaciones — Consulta de empresas y asociaciones

> Repositorio: BusTrack API · Estado: Pendiente · Fecha: 2026-10-30.

## 1. Objetivo

Especificar la consulta de empresas, asociaciones y medios de transporte.

## 2. Contexto

No existe endpoint ni modelo de empresas.

## 3. Alcance

**Incluye:**

- Listado y detalle.
- Asociaciones.
- Medios de transporte.
- Filtros y paginación.

**No incluye:**

- Cambiar el stack objetivo.
- Desplegar recursos o ejecutar migraciones en esta fase.

## 4. Requisitos funcionales

1. Datos provienen de la API/Cosmos.
2. Listados paginados.
3. Detalle con asociaciones y medios.
4. Sin datos inventados.

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
