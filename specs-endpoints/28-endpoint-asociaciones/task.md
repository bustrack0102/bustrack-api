# Tareas — 28-endpoint-asociaciones — Endpoint de cooperativas y asociaciones de transporte

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Definir contrato y DTO

- Crear `AsociacionDto` y validador de parámetros.
- Declarar signatura en `AsociacionesController`.

**Archivos:** API_SDD/Dtos/Transporte/AsociacionDto.cs, API_SDD/Controllers/V1/AsociacionesController.cs

**Terminado cuando:** el DTO y el endpoint estén estructurados según el estándar.

### T02. Pruebas de consulta y paginación

- Validar paginación correcta, cálculo de `totalCount` y filtrado por nombre.
- Validar rechazo de `pageSize > 50` con código 400.

**Archivos:** UnitTest/Transporte/AsociacionesTests.cs

**Terminado cuando:** las pruebas unitarias pasen satisfactoriamente.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

