# 07-modelo-de-dominio-de-transporte — Modelo de dominio de transporte

> Repositorio: BusTrack API · Estado: Pendiente · Fecha: 2026-10-30.

## 1. Objetivo

Definir agregados, relaciones y reglas del dominio de transporte antes de implementar consultas.

## 2. Contexto

El dominio no existe; solo hay foundation, usuarios y auth.

## 3. Alcance

**Incluye:**

- Empresas y asociaciones.
- Medios de transporte.
- Rutas y paradas.
- Destinos.
- Horarios y tarifas.
- Calificaciones.

**No incluye:**

- Rastreo GPS.
- Pagos.
- Reservas.

## 4. Requisitos funcionales

1. Modelar empresas/asociaciones y medios.
2. Modelar rutas, paradas y destinos.
3. Modelar horarios y tarifas.
4. Asociar calificaciones verificadas.

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
