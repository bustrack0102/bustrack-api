# Tareas — 38-endpoint-admin-empresas — Endpoint de administración de empresas de transporte

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTOs y validadores de empresas

- Diseñar `CrearEmpresaDto`, `ActualizarEmpresaDto`.
- Implementar validadores FluentValidation para campos requeridos y formatos.

**Archivos:** API_SDD/Dtos/Transporte/CrearEmpresaDto.cs, API_SDD/Validators/Transporte/CrearEmpresaValidator.cs

**Terminado cuando:** las clases compilen y pasen validaciones de sintaxis.

### T02. Pruebas de autorización y CRUD

- Probar rechazo 401 si no hay token y 403 si el rol no es Admin.
- Validar flujo de creación (201), actualización (200) y baja lógica (204).

**Archivos:** UnitTest/Transporte/EmpresasAdminTests.cs

**Terminado cuando:** las pruebas demuestren control de roles y consistencia de datos.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.

