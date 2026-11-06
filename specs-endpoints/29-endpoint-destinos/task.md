# Tareas — 29-endpoint-destinos — Endpoint de destinos y terminales

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DestinoDto y Controlador

- Diseñar DTO con campos de nombre, municipio, departamento y coordenadas geográficas.
- Implementar endpoint GET paginado en `DestinosController`.

**Archivos:** API_SDD/Dtos/Transporte/DestinoDto.cs, API_SDD/Controllers/V1/DestinosController.cs

**Terminado cuando:** el endpoint compile y devuelva lista paginada de destinos.

### T02. Pruebas de validación y filtrado

- Validar límites de paginación y orden alfabético.
- Verificar consistencia de coordenadas de geolocalización.

**Archivos:** UnitTest/Transporte/DestinosTests.cs

**Terminado cuando:** las pruebas unitarias pasen en verde.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

