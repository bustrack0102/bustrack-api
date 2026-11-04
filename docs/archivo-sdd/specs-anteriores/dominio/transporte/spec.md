# Dominio de transporte (empresas, rutas, paradas, destinos, horarios, tarifas, calificaciones)

> Repositorio: BusTrack API · Ruta: `specs/dominio/transporte/` · Estado: Pendiente · Fecha: 2026-10-30.

## 1. Identificador y nombre

- **ID:** `dominio/transporte`
- **Nombre:** Dominio de transporte (empresas, rutas, paradas, destinos, horarios, tarifas, calificaciones)
- **Repositorio:** BusTrack API

## 2. Objetivo

Especificar el dominio de transporte que la API debe exponer a Flutter y Angular.

## 3. Contexto

El código actual solo cubre foundation, usuarios y auth; el dominio no existe.

## 4. Alcance

**Incluye:**

- Empresas y asociaciones.
- Medios de transporte.
- Rutas y paradas.
- Destinos.
- Horarios.
- Tarifas.
- Calificaciones.

**No incluye:**

- Rastreo GPS en tiempo real.
- Rutas no registradas.
- Cálculo de llegadas en vivo.
- Pagos.

## 5. Requisitos funcionales

1. Obtener datos dinámicamente de la API/Cosmos.
2. Asociar calificaciones a empresas/servicios verificados.
3. Fundamentar recomendaciones en datos reales.
4. Mostrar en el mapa solo rutas/paradas/destinos registrados.

## 6. Requisitos no funcionales

1. Documentación verificable y sin secretos.
2. Cambios incrementales y reversibles.
3. Trazabilidad spec -> tarea -> prueba.

## 7. Reglas de negocio

- La API es la única fuente autorizada de información de transporte.
- Ningún cliente (Flutter/Angular) accede a Cosmos DB; todo pasa por la API.
- Cosmos DB for NoSQL sustituirá a EF Core/SQL Server como persistencia objetivo.

## 8. Criterios de aceptación

- [ ] Cada entidad tiene DTO, validador, servicio y endpoint documentado.
- [ ] Listados paginados.
- [ ] Errores uniformes.
- [ ] Pruebas de contrato.

## 9. Restricciones

- No implementar en esta fase.
- No inventar esquemas existentes.

## 10. Dependencias

- specs/02-arquitectura/
- specs/migraciones/cosmos-db/
- docs/contratos-integracion.md

## 11. Trazabilidad

- Plan: `plan.md` · Tareas: `task.md` · Diagramas: `chart.md`.
- Estado real: `../../Memoria.md`.
- Contratos: `../../docs/contratos-integracion.md`.
