# Tareas — 31-endpoint-ruta-individual — Endpoint de detalle de ruta

> Spec: `spec.md` · Plan: `plan.md` · Estado: Implementado (validado por build Release + `UnitTest`; integración pendiente de Docker).

## Actividades

### T01. Modelar DTO y acción en RutasController

- Definir `RutaDetalleDto` con trazado geográfico.
- Implementar signatura `GET /api/v1/rutas/{id}`.

**Archivos:** API_SDD/Dtos/Transporte/RutaDetalleDto.cs, API_SDD/Controllers/V1/RutasController.cs

**Terminado cuando:** la acción devuelva el detalle de la ruta con código 200.

### T02. Pruebas de contrato y trazado

- Validar formato de coordenadas GeoJSON válidas (longitud, latitud).
- Comprobar respuesta 404 para identificador inexistente.

**Archivos:** UnitTest/Transporte/RutasTests.cs

**Terminado cuando:** las pruebas unitarias certifiquen la serialización y códigos de error.

## Cierre

- [x] Contrato verificado contra el código.
- [x] Pruebas de éxito y error.
- [x] Documentación de consumidores actualizada.
- [x] Sin secretos ni PII en ejemplos.

