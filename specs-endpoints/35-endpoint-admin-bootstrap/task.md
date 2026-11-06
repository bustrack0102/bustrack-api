# Tareas — 35-endpoint-admin-bootstrap — Endpoint de bootstrap inicial de administrador

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar endpoint y servicio de Bootstrap

- Definir `BootstrapRequestDto` y validador con FluentValidation.
- Crear `BootstrapController` con validación de clave y verificación de existencia de admins.

**Archivos:** API_SDD/Dtos/Admin/BootstrapRequestDto.cs, API_SDD/Controllers/V1/BootstrapController.cs

**Terminado cuando:** el endpoint valide la clave de bootstrap y cree el usuario con rol Admin.

### T02. Pruebas de unicidad y denegación

- Probar intento de bootstrap con clave incorrecta (401).
- Probar segundo intento de bootstrap tras haber creado el primer admin (409).

**Archivos:** UnitTest/Admin/BootstrapTests.cs

**Terminado cuando:** las pruebas unitarias y de integración confirmen el bloqueo permanente.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

