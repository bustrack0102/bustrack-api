# Tareas — 39-endpoint-admin-rutas — Endpoint de administración de rutas y paradas

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTOs y validación de trazado

- Crear `CrearRutaDto` con lista ordenada de paradas y GeoJSON.
- Desarrollar validador FluentValidation de ruta y coordenadas.

**Archivos:** API_SDD/Dtos/Transporte/CrearRutaDto.cs, API_SDD/Validators/Transporte/CrearRutaValidator.cs

**Terminado cuando:** el validador rechace GeoJSONs con sintaxis rota o sin paradas.

### T02. Pruebas de integración de alta de rutas

- Probar creación exitosa con 201 y cabecera `Location`.
- Probar rechazo con código 409 cuando el código de ruta ya existe.

**Archivos:** UnitTest/Transporte/RutasAdminTests.cs

**Terminado cuando:** las pruebas unitarias y de integración pasen satisfactoriamente.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

