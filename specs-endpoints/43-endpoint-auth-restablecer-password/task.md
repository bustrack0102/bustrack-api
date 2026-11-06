# Tareas — 43-endpoint-auth-restablecer-password — Endpoint de restablecimiento de contraseña (EP-AU-06)

> Spec: `spec.md` · Plan: `plan.md` · Estado: Propuesto.

## Actividades

### T01. Modelar DTO y validador de restablecimiento

- Crear `RestablecerPasswordRequestDto`.
- Diseñar validador FluentValidation comprobando complejidad y coincidencia de password.

**Archivos:** API_SDD/Dtos/Auth/RestablecerPasswordRequestDto.cs, API_SDD/Validators/Auth/RestablecerPasswordValidator.cs

**Terminado cuando:** las clases compilen y validen los campos de entrada.

### T02. Pruebas de consumo único y revocación de sesiones

- Probar actualización correcta con token vigente y no usado.
- Probar fallo al intentar reutilizar el mismo token una segunda vez.
- Verificar que los refresh tokens activos del usuario queden marcados como revocados.

**Archivos:** UnitTest/Auth/RestablecerPasswordTests.cs

**Terminado cuando:** la suite de pruebas unitarias y de integración valide las tres condiciones.

## Cierre

- [ ] Contrato verificado contra el código.
- [ ] Pruebas de éxito y error.
- [ ] Documentación de consumidores actualizada.
- [ ] Sin secretos ni PII en ejemplos.
