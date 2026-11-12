# Tareas — 32-endpoint-parada-individual — Endpoint de detalle y cercanía de paradas

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado (validado por build Release + `UnitTest`; integración pendiente de Docker).

## Actividades

### T01. Modelar DTOs y ParadasController

- Definir `ParadaDetalleDto` y `ParadaCercanaDto`.
- Declarar rutas `GET /api/v1/paradas/{id}` y `GET /api/v1/paradas/cercanas`.

**Archivos:** API_SDD/Dtos/Transporte/ParadaDetalleDto.cs, API_SDD/Controllers/V1/ParadasController.cs

**Terminado cuando:** las rutas estén definidas y los DTOs estructurados.

### T02. Pruebas de geocálculo y filtros

- Validar filtros de latitud (-90 a 90) y longitud (-180 a 180).
- Verificar que el cálculo de distancia sea coherente y ordenado.

**Archivos:** UnitTest/Transporte/ParadasTests.cs

**Terminado cuando:** la suite de pruebas unitarias valide las restricciones de coordenadas.

## Cierre

- [x] Contrato verificado contra el código.
- [x] Pruebas de éxito y error.
- [x] Documentación de consumidores actualizada.
- [x] Sin secretos ni PII en ejemplos.

