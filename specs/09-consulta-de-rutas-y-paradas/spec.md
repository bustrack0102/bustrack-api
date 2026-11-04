# 09-consulta-de-rutas-y-paradas — Consulta de rutas y paradas

> Repositorio: BusTrack API · Estado: Pendiente · Fecha: 2026-10-30.

## 1. Objetivo

Especificar la consulta de rutas con sus paradas ordenadas.

## 2. Contexto

No existe modelo ni endpoint de rutas/paradas.

## 3. Alcance

**Incluye:**

- Listado de rutas por destino/empresa.
- Detalle de ruta.
- Paradas ordenadas.
- Filtros.

**No incluye:**

- Cambiar el stack objetivo.
- Desplegar recursos o ejecutar migraciones en esta fase.

## 4. Requisitos funcionales

1. Ruta incluye paradas en orden.
2. Filtros por empresa y destino.
3. Paginación.
4. Sin rutas no registradas.

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
